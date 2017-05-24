using System;
using System.Collections.Generic;
using System.Linq;
using CollectorPackage.Aoicollector.Inspection.Model;
using System.IO;
using CollectorPackage.Aoicollector.IAServer;
using System.Threading.Tasks;
using AOICollector.Src.Redis;

namespace CollectorPackage.Aoicollector.Inspection
{
    public class Panel: Revision
    {
        public int panelNro = 0;

        public int panelId = 0;
        public string op;

        public string maquina;
        public string programa;
        public int totalBloques;
       
        // Fecha y hora de AOI
        public string fecha;
        public string hora;
        // Fecha y hora de Inspeccion
        public string inspFecha;
        public string inspHora;

        public PcbInfo pcbInfo = new PcbInfo();
        public Machine machine = new Machine();
        public History history = new History();
        public List<Bloque> bloqueList = new List<Bloque>();

        public bool pendiente = false;
        public bool pendienteDelete = false;
        public string spMode = "update";

        // RNS
        public string csvFile;
       // public string csvFileInspectionResultPath;
        public FileInfo csvFilePath;
        public DateTime csvDatetime;
        public DateTime csvDateCreate;
        public DateTime csvDateSaved;
        public string rncInspectionProgramVersionName;
        public DirectoryInfo rnsInspectionResultDirectory;
        public DirectoryInfo rnsCurrentInspectionResultDirectory;

        // VTS
        public int vtsOracleInspId;
        public int vtsOraclePgItemId;

        // VTWIN
        public int vtwinSaveMachineId;
        public int vtwinTestMachineId;
        public int vtwinProgramNameId;
        public int vtwinRevisionNo;
        public int vtwinSerialNo;
        public int vtwinLoadCount;

        // ZENITH
        public string zenithPcbguid;
        public string zenithImageDb;
        public string zenithResultDb;
        public string zenithPcbRepair;

        /// <summary>
        /// Procesa la informacion de cada bloque segun sus detalles
        /// </summary>
        /// <returns></returns>
        public void MakeRevisionToAll()
        {
            foreach (Bloque bloque in bloqueList)
            {
                bloque.detailList = detailList.Where(n => n.bloqueId == bloque.bloqueId).ToList();
                bloque.MakeRevision();
            }

            totalBloques = bloqueList.Count;

            if(totalBloques == 0)
            {
                totalBloques = pcbInfo.bloques;
            }

            // Analiza el panel general
            MakeRevision();
        }

        /// <summary>
        /// Obtiene datos de panel desde el webservice
        /// </summary>
        /// <returns>InspectionService</returns>
        public PanelService GetBarcodeInfoFromIAServer()
        {           
            PanelService panelService = new PanelService();

            // Por ahora no verifica si ya fue declarado....
            panelService.GetPanelInfo(barcode, false);
            if (panelService.error == null)
            {
                if (panelService.result.panel != null)
                {
                    panelId = panelService.result.panel.id;
                    op = panelService.result.panel.inspected_op;                   

                    machine.LogBroadcast("notify",
                        string.Format("El panel ID: ({0}) tiene OP Asignada: {1}", panelId, op)
                    );
                }
                else
                {
                    machine.LogBroadcast("warning",
                        string.Format("El panel no fue registrado en IAServer ({0})", barcode)
                    );
                }
            } else {
                machine.log.stack(
                    string.Format("+ Stack Error en la verificacion de panel en IAServer ({0}) ", barcode
                ), this, panelService.error);
            }

            if (panelId == 0)
            {
                spMode = "insert";
            }

            return panelService;
        }

        // LLAMADA EN FASE BETA, necesita testing, pero en modo async podria aumentar mucho la velocidad de inspeccion
        private async Task<PanelService> AsyncGetBarcodeInfoFromIAServer()
        {
            PanelService panelService = await Task.Run(() => GetBarcodeInfoFromIAServer());
            return panelService;
        }
    }
}
