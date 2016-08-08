using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection.Model
{
    // Datos de maquina en DB MySql
    public class Machine
    {
        public static List<Machine> list = new List<Machine>();
        public RichLog log;
        public RichLog glog;

        public int mysql_id;
        public int oracle_id;

        public string maquina;
        public string linea;
        public string smd;
        public string tipo;
        public string ultima_inspeccion;
        public string line_barcode;
        public bool active = true;

        public string op;
        public bool opActive;
        public bool opDeclara = false;

        public void Ping()
        {
            MySqlConnector sql = new MySqlConnector();
            string query = @"UPDATE  `aoidata`.`maquina` SET  `ping` =  NOW() WHERE  `id` = " + mysql_id + " LIMIT 1;";
            DataTable sp = sql.Select(query);
        }

        public static void Download()
        {
            list = new List<Machine>();

            string query = @"
            SELECT
                m.id,
                p.barcode,
                m.maquina,
                m.linea,
                m.tipo,
                m.ultima_inspeccion,
                m.active 
            from
                aoidata.maquina as m
            left join aoidata.produccion p ON m.id = p.id_maquina
            ";

            MySqlConnector sql = new MySqlConnector();
            DataTable dt = sql.Select(query);
            if (sql.rows)
            {
                foreach (DataRow r in dt.Rows)
                {
                    Machine mac = new Machine();
                    mac.mysql_id = int.Parse(r["id"].ToString());
                    mac.maquina = r["maquina"].ToString();
                    mac.linea = r["linea"].ToString();
                    mac.smd = "SMD-"+mac.linea;
                    mac.tipo = r["tipo"].ToString();
                    mac.ultima_inspeccion = r["ultima_inspeccion"].ToString();
                    mac.line_barcode = r["barcode"].ToString();
                    mac.active = bool.Parse(r["active"].ToString());
                    Machine.list.Add(mac);
                }
            }
        }

        public static void UpdateInspectionDate(int id, DateTime custom_date)
        {
            MySqlConnector sql = new MySqlConnector();
            string query = @"CALL sp_updateInspeccionMaquina(" + id + ",'" + custom_date.ToString("yyyy-MM-dd HH:mm:ss") + "');";
            DataTable sp = sql.Select(query);
            if (sql.rows)
            {
                string ultima_inspeccion = sp.Rows[0]["ultima_inspeccion"].ToString();

                Machine update = Machine.list.Find(obj => obj.mysql_id == id);
                update.ultima_inspeccion = ultima_inspeccion;
            }
        }

        public static void UpdateInspectionDate(int id)
        {
            MySqlConnector sql = new MySqlConnector();
            string query = @"CALL sp_updateInspeccionMaquina(" + id + ", NOW());";
            DataTable sp = sql.Select(query);
            if (sql.rows)
            {
                string ultima_inspeccion = sp.Rows[0]["ultima_inspeccion"].ToString();

                Machine update = Machine.list.Find(obj => obj.mysql_id == id);
                update.ultima_inspeccion = ultima_inspeccion;
            }
        }

        public static int Total()
        {
            return list.Count();
        }

        public void LogBroadcast(string mode, string msg)
        {
            log.putLog(glog.putLog(msg, mode), mode);
        }
    }
}
