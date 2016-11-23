using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using CollectorPackage.Src.Database;

namespace CollectorPackage.Aoicollector.Inspection.Model
{
    public class Pendiente
    {
        // DATOS IASERVER
        public int idPendiente = 0;    // id pendiente en mysql 
        public int idPanel = 0;        // id panel en mysql 
        public int idMaquina;          // id en mysql (maquina)

        public string programa;
        public string barcode;
        public string fechaMaquina;
        public string vtwinProgramNameId;      // id de programa en oracle 
        public int vtwinTestMachineId = 0;    // id de maquina en oracle 

        public static void Delete(InspectionController ictrl)
        {
            ictrl.machine.LogBroadcast("debug",
                string.Format("+ Pendiente.Delete({0}) ", ictrl.barcode)
            );

            string query = string.Format("CALL sp_removeProcesarPendient('{0}')", ictrl.barcode);
            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            bool rs = sql.NonQuery(query);
        }

        public static void Save(InspectionController ictrl)
        {
            DateTime customDate = DateTime.Parse(ictrl.fecha + " " + ictrl.hora);
            ictrl.machine.LogBroadcast("debug",
                string.Format("+ Pendiente.Save({0}) ", ictrl.barcode)
            );

            string query = string.Format("CALL sp_addProcesarPendient('{0}', '{1}', '{2}', '{3}', '{4}', '{5}');",
                ictrl.barcode,
                customDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ictrl.machine.mysql_id,
                ictrl.programa,
                ictrl.vtwinProgramNameId,
                ictrl.vtwinTestMachineId
            );

            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            bool rs = sql.NonQuery(query);
        }
        
        public static List<Pendiente> Download(string machineNameKey)
        {
            List<Pendiente> pendlist = new List<Pendiente>();
            string query = @"
            SELECT          
                p.id,      
                p.barcode,
                CAST(p.fecha_maquina as CHAR) as fecha_maquina,
                p.id_maquina,
                p.programa,
                p.vtwin_program_name_id,
                p.vtwin_test_machine_id
                
            from
            aoidata.procesar_pendiente p,
            aoidata.maquina m            
            
            where 
            m.id = p.id_maquina and
            m.active = 1 and
            m.tipo = '" + machineNameKey + "' ";

            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            DataTable dt = sql.Query(query);
            if (sql.rows)
            {
                foreach (DataRow r in dt.Rows)
                {
                    Pendiente ipen = new Pendiente();
                    ipen.idPendiente = int.Parse(r["id"].ToString());
                    ipen.barcode = r["barcode"].ToString();
                    ipen.fechaMaquina = r["fecha_maquina"].ToString();
                    ipen.idMaquina = int.Parse(r["id_maquina"].ToString());
                    ipen.programa = r["programa"].ToString();
                    ipen.vtwinTestMachineId = int.Parse(r["vtwin_test_machine_id"].ToString());
                    ipen.vtwinProgramNameId = r["vtwin_program_name_id"].ToString();
                    pendlist.Add(ipen);
                }
            }

            return pendlist;
        }
    }
}
