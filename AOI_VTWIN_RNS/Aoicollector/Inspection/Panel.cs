using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using System.IO;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
{
    public class Panel: Revision
    {
        public int panelNro = 0;

        public int panelId = 0;

        public string op;
        public int puestoId;
        public int lineId;
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

        // RNS
        public string csvFile;
        public string csvFileInspectionResultPath;
        public FileInfo csvFilePath;
        public DateTime csvDatetime;
        public DateTime csvDateCreate;
        public DateTime csvDateSaved;

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

            // Analiza el panel general
            MakeRevision();
        }
    }
}
