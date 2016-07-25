using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using AOI_VTWIN_RNS.Src.Database;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection.Model
{
    public class Pendiente
    {
        // DATOS IASERVER
        public int idPendiente = 0;    // id pendiente en mysql 
        public int idPanel = 0;        // id panel en mysql 
        public int idMaquina;          // id en mysql (maquina)
        
        // DATOS EN COMUN
        public string programa;
        public string barcode;
        public string endDate;

        // NECESARIO EN VTWIN
        public string programNameId;      // id de programa en oracle 
        public int testMachineId = 0;    // id de maquina en oracle 

        public static void Delete(int idPanel)
        {
            string query = "CALL sp_removePendient(" + idPanel + ")";
            MySqlConnector sql = new MySqlConnector();
            bool rs = sql.Ejecutar(query);
        }

        public static void Save(InspectionObject inspc, bool insert = true)
        {
            DateTime customDate = DateTime.Parse(inspc.fecha + " " + inspc.hora);
            if (insert)
            {
                string query = "CALL sp_addInspectionPendient('" + inspc.idPanel + "', '" + inspc.panelBarcode + "',  '" + customDate.ToString("yyyy-MM-dd HH:mm:ss") + "');";
                MySqlConnector sql = new MySqlConnector();
                bool rs = sql.Ejecutar(query);
            }
        }
        
        public static List<Pendiente> Download(string machineNameKey)
        {
            List<Pendiente> pendlist = new List<Pendiente>();
            string query = @"
            SELECT                
                ip.id,
                p.id as id_panel,
                DATE_FORMAT(ip.end_date,'%Y-%m-%d %H:%i:%s') as end_date,
                ip.barcode,
                p.test_machine_id,
                p.id_maquina,
                p.programa,
                p.program_name_id
                
            from
            aoidata.inspeccion_pendiente ip,
            aoidata.inspeccion_panel p ,           
            aoidata.maquina m            
            
            where 
            p.id = ip.id_panel  and
            m.id = p.id_maquina and
            m.tipo = '"+machineNameKey+"' ";

            MySqlConnector sql = new MySqlConnector();
            DataTable dt = sql.Select(query);
            if (sql.rows)
            {
                foreach (DataRow r in dt.Rows)
                {
                    Pendiente ipen = new Pendiente();
                    ipen.idPendiente = int.Parse(r["id"].ToString());
                    ipen.idPanel = int.Parse(r["id_panel"].ToString());
                    ipen.endDate = r["end_date"].ToString();
                    ipen.barcode = r["barcode"].ToString();
                    ipen.testMachineId = int.Parse(r["test_machine_id"].ToString());
                    ipen.idMaquina = int.Parse(r["id_maquina"].ToString());
                    ipen.programa = r["programa"].ToString();
                    ipen.programNameId = r["program_name_id"].ToString();
                    pendlist.Add(ipen);
                }
            }

            return pendlist;
        }
    }
}
