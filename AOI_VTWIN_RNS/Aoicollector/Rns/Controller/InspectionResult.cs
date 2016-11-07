using AOI_VTWIN_RNS.Aoicollector.Inspection;
using AOI_VTWIN_RNS.Src.Util.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AOI_VTWIN_RNS.Aoicollector.Rns.Controller
{
    class InspectionResult : InspectionTextFileHandler
    {
        public RnsPanel panel;
        public RnsInspection rnsi;

        public List<InspectionTextFileHeader> fileHeaders;

        public bool located = false;

        public InspectionResult(RnsPanel panel, RnsInspection rnsi)
        {
            this.panel = panel;
            this.rnsi = rnsi;

            FindProgramFolders();

            if (panel.rnsInspectionResultDirectory != null)
            {
                string filter = panel.maquina + "*" + panel.csvDateCreate.ToString("yyyyMMdd") + panel.csvDateCreate.ToString("HHmmss");
                panel.rnsCurrentInspectionResultDirectory = FilesHandler.GetFolders(filter, panel.rnsInspectionResultDirectory.FullName).FirstOrDefault();
                if (panel.rnsCurrentInspectionResultDirectory != null)
                {
                    panel.machine.LogBroadcast("debug", string.Format("+ Ruta de InspectionResult localizada ({0}) | InspectionResult: {1}",
                        panel.barcode,
                        panel.rncInspectionProgramVersionName
                    ));

                    fileHeaders = ContentFileToObject(panel.rnsCurrentInspectionResultDirectory);

                    located = true;
                }
                else
                {
                    panel.machine.LogBroadcast("debug", string.Format("+ Ruta de InspectionResult no encontrada"));
                }
            }
            else
            {
                panel.machine.LogBroadcast("debug", string.Format("+ Ruta a InspectionResult no encontrada"));
            }
        }

        /// <summary>
        /// Localiza las carpetas de InspectionResult del programa actual
        /// </summary>
        private void FindProgramFolders()
        {
            // Formateo la carpeta al tipo: "RVS-1234" y agrego al programa un comodin " * " 
            string RVS_filter = panel.maquina.Replace("VT-RNS", "RVS");

            string InspectionPath = Directory.GetParent(rnsi.aoiConfig.inspectionCsvPath).Parent.FullName;

            string InspeccionResultFolder = Path.Combine(InspectionPath, RVS_filter, "InspectionResult");

            if (panel.programa != null)
            {
                panel.rncInspectionProgramVersionName = SustractProgramFolderName();

                panel.machine.LogBroadcast("debug", string.Format("+ Buscando carpetas de inspeccion para el programa: {0}", panel.rncInspectionProgramVersionName));

                panel.rnsInspectionResultDirectory = FilesHandler.GetFolders(panel.rncInspectionProgramVersionName + "*" + "InspectionResult", InspeccionResultFolder).FirstOrDefault();
            }
            else {
                panel.machine.LogBroadcast("warning", string.Format("+ No esta difinido el nombre del programa, no se pueden buscar las carpetas de inspeccion"));
            }
        }

        /// <summary>
        /// Obtiene el nombre del programa y su version desde el nombre del CSV
        /// </summary>
        /// <returns></returns>
        private string SustractProgramFolderName()
        {
            // Solucion a problema de caracteres invalidos en barcode
            string regexValidator = String.Concat(panel.programa, @"_\d_");
            Match match = Regex.Match(panel.csvFile, regexValidator);
            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Obtiene los barcodes del panel
        /// </summary>
        public List<Bloque> GetBlockBarcodes()
        {
            List<Bloque> bloques = new List<Bloque>();

            if (located && fileHeaders.Count > 0)
            {
                InspectionTextFileHeader pcbinfo = fileHeaders.Where(o => o.header == "PcbInfo").FirstOrDefault();

                if (pcbinfo == null)
                {
                    rnsi.aoiLog.warning("No se localizo el header PcbInfo en InspectionResult");
                }
                else
                {
                    List<InspectionTextFileHeader> res = fileHeaders.Where(o => o.header == "BlockBarcode").ToList();
                    if (res.Count() > 0)
                    {
                        // Bloques disponibles!
                        foreach (InspectionTextFileHeader bar in res)
                        {
                            string barcode = bar.FindAtrributeValue("szBlockBarcode");
                            string block = bar.FindAtrributeValue("nComponentBlockNo");

                            Bloque bk = new Bloque(barcode);
                            bk.bloqueId = int.Parse(block);
                            bloques.Add(bk);
                        }
                    }
                }
            }  

            if (bloques.Count == 0)
            {
                string barcode = panel.barcode;
                List<int> posibleBlockId = panel.detailList.Select(o => o.bloqueId).Distinct().ToList();
                string blockId = "1";
                if (posibleBlockId.Count > 0)
                {
                    blockId = posibleBlockId.First().ToString();
                }

                Bloque bk = new Bloque(barcode);
                bk.bloqueId = int.Parse(blockId);
                bloques.Add(bk);
            }

            return bloques;
        }
    }
}
