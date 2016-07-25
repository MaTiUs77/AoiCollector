using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using AOI_VTWIN_RNS.Src.Util.Files;
using AOI_VTWIN_RNS.Aoicollector.Inspection;
using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Src.Util.Crypt;

namespace AOI_VTWIN_RNS.Aoicollector.Rns.Controller
{
    class InspectionResultFile
    {
        public RnsInspection rnsi;

        // Salto de linea en archivo InspectionResult.txt
        private char FILAS = '\n';

        public InspectionResultFile(RnsInspection _rnsi)
        {
            rnsi = _rnsi;
        }

        #region PRIVATE
        /// <summary>
        ///  Obtengo informacion de Headers de archivo InspectionResult
        /// </summary>
        /// <param name="InspectionResultFile">Archivo .txt</param>
        /// <returns></returns>
        private List<InspectionResultObject> GetHeaders(string inspectionResultTextFile)
        {
            string contenido = FilesHandler.ReadFile(inspectionResultTextFile);
            contenido += "\n----DUMMY";
            string[] lineas = contenido.Split(FILAS);

            List<InspectionResultObject> headers = new List<InspectionResultObject>();
            InspectionResultObject obj = new InspectionResultObject();

            // Guardo datos de headers, variables y valores en un diccionario
            foreach (string txt in lineas)
            {
                if (txt.Contains("----"))
                {
                    if (!obj.header.Equals(""))
                    {
                        headers.Add(obj);
                    }
                    obj = new InspectionResultObject();
                    obj.header = txt.ToString().Replace('\r', ' ').Replace('-', ' ').Trim();
                }
                else
                {
                    string[] sp = txt.Split(':');

                    if (sp.Length > 1)
                    {
                        string fVar = sp[0].ToString().Replace('\r', ' ').Trim();
                        string fVal = sp[1].ToString().Replace('\r', ' ').Trim();
                        obj.value.Add(fVar, fVal);
                    }
                }
            }
            return headers;
        }
        #endregion

        /// <summary>
        /// Obtiene los codigos de bloque, analizando el archivo InspectionResult.txt segun la maquina  
        /// </summary>
        /// <param name="insp">Inspection object</param>
        /// <returns></returns>
        public List<BlockBarcode> GetBlockBarcodes()
        {
            List<BlockBarcode> blockData = new List<BlockBarcode>();

            // Formateo la carpeta al tipo: "RVS-1234" y agrego al programa un comodin " * " 
            string RVS_filter = rnsi.inspectionObj.maquina.Replace("VT-RNS", "RVS");
            string InspectionPath = Directory.GetParent(rnsi.aoiConfig.inspectionCsvPath).Parent.FullName;
            string InspeccionResultFolder = Path.Combine(InspectionPath, RVS_filter);
            InspeccionResultFolder = Path.Combine(InspeccionResultFolder, "InspectionResult");

            int first = rnsi.inspectionObj.csvFile.IndexOf(rnsi.inspectionObj.programa); //Busca el nombre del progarma en el nombre del archivo
            int last = rnsi.inspectionObj.csvFile.LastIndexOf("_0_"); // Hasta _0_
            //El nombre de la carpeta de InspectionResult deberia ser
            if (last >= 0)
            {
                rnsi.inspectionObj.csvFileInspectionResultPath = String.Concat(rnsi.inspectionObj.csvFile.Substring(first, last - first), "_0_");
            }
            else {
                Log.Sys("GetBlockBarcodes() " + rnsi.inspectionObj.pcbInfo.programa + " no fue posible encontrar la ruta de InspectionResult", "error");
            }
            // Busco en la carpeta InspectionResult el programa correspondiente a la maquina.
            DirectoryInfo programFolder = FilesHandler.GetFolders(rnsi.inspectionObj.csvFileInspectionResultPath + "*" + "InspectionResult", InspeccionResultFolder).FirstOrDefault();

            if (programFolder != null)
            {
                // Busco la inspeccion realizada para este CSV.
                string InspectionResultFilter = rnsi.inspectionObj.maquina + "*" + rnsi.inspectionObj.csvDateCreate.ToString("yyyyMMdd") + rnsi.inspectionObj.csvDateCreate.ToString("HHmmss");
                DirectoryInfo InspectionFolderForCsv = FilesHandler.GetFolders(InspectionResultFilter, programFolder.FullName).FirstOrDefault();
                if (InspectionFolderForCsv != null)
                {
                    blockData = GetBarcodes(InspectionFolderForCsv);

                    if (blockData.Count == 0)
                    {
                        string barcode = rnsi.inspectionObj.panelBarcode;
                        List<int> posibleBlockId = rnsi.inspectionObj.detailList.Select(o => o.bloqueId).Distinct().ToList();
                        string blockId = "1";
                        if(posibleBlockId.Count>0)
                        {
                            blockId = posibleBlockId.First().ToString();
                        }

                        BlockBarcode bk = new BlockBarcode();
                        bk.bloqueId = int.Parse(blockId);
                        bk.barcode = barcode;
                        bk.validateBarcode();
                        blockData.Add(bk);
                    }

                    return blockData;
                }
                else
                {
                    rnsi.aoiLog.Area("Barcode: (" + rnsi.inspectionObj.panelBarcode + ") - No se encontro: " + InspectionResultFilter, "atencion");
                }
            }
            else
            {
                rnsi.aoiLog.Area("Barcode: (" + rnsi.inspectionObj.panelBarcode + ") - No se localizo la carpeta de inspeccion: " + RVS_filter + " de " + rnsi.inspectionObj.csvFileInspectionResultPath, "atencion");
            }
            return blockData;
        }

        public List<BlockBarcode> GetBarcodes(DirectoryInfo InspectionFile)
        {
            List<BlockBarcode> blockData = new List<BlockBarcode>();

            string fileUrl = InspectionFile.FullName + @"\InspectionResult.txt";
            if (File.Exists(fileUrl))
            {
                List<InspectionResultObject> headers = GetHeaders(fileUrl);
                InspectionResultObject pcbinfo = headers.Where(o => o.header == "PcbInfo").FirstOrDefault();

                if (pcbinfo == null)
                {
                    rnsi.aoiLog.Area("No fue posible conseguir PCINFO de " + fileUrl, "atencion");
                }
                else
                {
                    List<InspectionResultObject> res = headers.Where(o => o.header == "BlockBarcode").ToList();
                    if (res.Count() > 0)
                    {
                        // Bloques disponibles!
                        foreach (InspectionResultObject bar in res)
                        {
                            string barcode = DictionaryKeyValue("szBlockBarcode", bar);
                            string block = DictionaryKeyValue("nComponentBlockNo", bar);

                            BlockBarcode bk = new BlockBarcode();
                            bk.bloqueId = int.Parse(block);
                            bk.barcode = barcode;
                            bk.validateBarcode();        
                            blockData.Add(bk);
                        }
                    }
                }
            }
            return blockData;
        }
        
        public string DictionaryKeyValue(string key, InspectionResultObject obj)
        {
            string rt = obj.value.Where(o => o.Key == key).FirstOrDefault().Value;
            return rt;
        }
    }
}
