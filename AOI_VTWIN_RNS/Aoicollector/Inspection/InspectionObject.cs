using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using System.IO;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
{
    public class InspectionObject
    {
        public int totalErrores = 0;
        public int totalErroresReales = 0;
        public int totalErroresFalsos = 0;

        public int panelNro = 0;

        // id panel en MYSQL.
        public int idPanel = 0;
        public string op;
        public string puestoId;
        public string lineId;

        public string maquina;
        public string programa;

        public string panelBarcode;
        public string tipoPanelBarcode;
        // Fecha y hora de AOI
        public string fecha;
        public string hora;
        // Fecha y hora de Inspeccion
        public string inspFecha;
        public string inspHora;

        public string revisionAoi;
        public string revisionIns;

        public bool pendiente = false;
        public bool pendienteDelete = false;

        public PcbInfo pcbInfo = new PcbInfo();
        public Machine machine = new Machine();
        public History history = new History();
        public List<Detail> detailList = new List<Detail>();
        public List<BlockBarcode> blockBarcodeList = new List<BlockBarcode>();

        // RNS
        public string csvFile;
        public string csvFileInspectionResultPath;
        public FileInfo csvFilePath;
        public DateTime csvDatetime;
        public DateTime csvDateCreate;
        public DateTime csvDateSaved;

        // VTS500
        public int vts500_oracle_insp_id;
        public int vts500_oracle_pg_item_id;

        // VTWIN
        public int vtwin_save_machine_id;
        public int vtwin_test_machine_id;
        public int vtwin_program_name_id;
        public int vtwin_revision_no;
        public int vtwin_serial_no;
        public int vtwin_load_count;

        public void ProccessPanelStatus()
        {
            foreach (BlockBarcode blockBarcode in blockBarcodeList)
            {
                blockBarcode.detailList = detailList.Where(n => n.bloqueId == blockBarcode.bloqueId).ToList();
                blockBarcode.processDetail();
            }

            IEnumerable<Detail> block_REAL = detailList.Where(obj => obj.estado == "REAL");
            IEnumerable<Detail> block_FALSO = detailList.Where(obj => obj.estado == "FALSO");
            IEnumerable<Detail> block_PENDIENTE = detailList.Where(obj => obj.estado == "PENDIENTE");

            totalErroresFalsos = block_FALSO.Count() + block_PENDIENTE.Count();
            totalErroresReales = block_REAL.Count();
            totalErrores = totalErroresFalsos + totalErroresReales;

            if (totalErrores == 0)
            {
                revisionIns = "OK"; 
                revisionAoi = "OK";
            }
            else
            {
                revisionAoi = "NG";
                revisionIns = "NG";

                if (totalErroresReales == 0)
                {
                    revisionIns = "OK";
                }

                if (totalErroresFalsos == 0) {
                    revisionAoi = "OK";
                }
            }
        }
    }
}
