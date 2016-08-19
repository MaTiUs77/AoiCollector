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
        public RnsPanel panel;

        // Salto de linea en archivo InspectionResult.txt
        private char FILAS = '\n';

        public InspectionResultFile(RnsPanel panel, RnsInspection rnsi)
        {
            this.panel = panel;
            this.rnsi = rnsi;
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
        public List<Bloque> GetBlockBarcodes()
        {
            List<Bloque> bloques = new List<Bloque>();

            // Formateo la carpeta al tipo: "RVS-1234" y agrego al programa un comodin " * " 
            string RVS_filter = panel.maquina.Replace("VT-RNS", "RVS");
            string InspectionPath = Directory.GetParent(rnsi.aoiConfig.inspectionCsvPath).Parent.FullName;
            string InspeccionResultFolder = Path.Combine(InspectionPath, RVS_filter);
            InspeccionResultFolder = Path.Combine(InspeccionResultFolder, "InspectionResult");

            int first = panel.csvFile.IndexOf(panel.programa); //Busca el nombre del progarma en el nombre del archivo
            int last = panel.csvFile.LastIndexOf("_0_"); // Hasta _0_
            //El nombre de la carpeta de InspectionResult deberia ser
            if (last >= 0)
            {
                panel.csvFileInspectionResultPath = String.Concat(panel.csvFile.Substring(first, last - first), "_0_");
                panel.machine.LogBroadcast("debug", string.Format("+ Buscando carpeta de InspectionResult: {0}", panel.csvFileInspectionResultPath));
            }
            else {
                panel.machine.LogBroadcast("warning", string.Format("+ No fue posible encontrar la ruta de InspectionResult"));
            }
            // Busco en la carpeta InspectionResult el programa correspondiente a la maquina.
            DirectoryInfo programFolder = FilesHandler.GetFolders(panel.csvFileInspectionResultPath + "*" + "InspectionResult", InspeccionResultFolder).FirstOrDefault();

            if (programFolder != null)
            {
                // Busco la inspeccion realizada para este CSV.
                string InspectionResultFilter = panel.maquina + "*" + panel.csvDateCreate.ToString("yyyyMMdd") + panel.csvDateCreate.ToString("HHmmss");
                DirectoryInfo InspectionFolderForCsv = FilesHandler.GetFolders(InspectionResultFilter, programFolder.FullName).FirstOrDefault();
                if (InspectionFolderForCsv != null)
                {
                    panel.machine.LogBroadcast("debug", string.Format("+ Ruta de inspeccion localizada ({0}) | InspectionResult: {1}", panel.barcode, panel.csvFileInspectionResultPath));

                    bloques = GetBloquesFromInspectionResultFile(InspectionFolderForCsv);

                    if (bloques.Count == 0)
                    {
                        string barcode = panel.barcode;
                        List<int> posibleBlockId = panel.detailList.Select(o => o.bloqueId).Distinct().ToList();
                        string blockId = "1";
                        if(posibleBlockId.Count>0)
                        {
                            blockId = posibleBlockId.First().ToString();
                        }

                        Bloque bk = new Bloque(barcode);
                        bk.bloqueId = int.Parse(blockId);
                        bloques.Add(bk);
                    }

                    return bloques;
                }
                else
                {
                    panel.machine.LogBroadcast("warning", string.Format("Barcode: ({0}) - No se localizo el CSV de inspeccion | Filtro: {1}", panel.barcode, InspectionResultFilter));
                }
            }
            else
            {
                panel.machine.LogBroadcast("warning", string.Format("Barcode: ({0}) - No se localizo la carpeta de inspeccion: {1} de {2}", panel.barcode , RVS_filter, panel.csvFileInspectionResultPath));
            }
            return bloques;
        }

        protected List<Bloque> GetBloquesFromInspectionResultFile(DirectoryInfo InspectionFile)
        {
            List<Bloque> bloques = new List<Bloque>();

            string fileUrl = InspectionFile.FullName + @"\InspectionResult.txt";
            if (File.Exists(fileUrl))
            {
                List<InspectionResultObject> headers = GetHeaders(fileUrl);
                InspectionResultObject pcbinfo = headers.Where(o => o.header == "PcbInfo").FirstOrDefault();

                if (pcbinfo == null)
                {
                    rnsi.aoiLog.warning("No fue posible conseguir PCINFO de " + fileUrl);
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

                            Bloque bk = new Bloque(barcode);
                            bk.bloqueId = int.Parse(block);
                            bloques.Add(bk);
                        }
                    }
                }
            }
            return bloques;
        }
        
        protected string DictionaryKeyValue(string key, InspectionResultObject obj)
        {
            string rt = obj.value.Where(o => o.Key == key).FirstOrDefault().Value;
            return rt;
        }
    }
}
