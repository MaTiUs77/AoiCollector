using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using CollectorPackage.Src.Database;

namespace CollectorPackage.Aoicollector.Inspection.Model
{
    // Datos de Faultcodes en Mysql
    class Faultcode
    {
        public static List<Faultcode> list = new List<Faultcode>();

        public int id;
        public string faultcode;
        public string descripcion;

        /// <summary>
        /// Descarga la lista de faultcodes del servidor
        /// </summary>
        public static void Download()
        {
            list = new List<Faultcode>();

            string query = @"SELECT id,faultcode,descripcion FROM aoidata.rns_faultcode ";

            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            DataTable dt = sql.Query(query);
            if (sql.rows)
            {
                foreach (DataRow r in dt.Rows)
                {
                    Faultcode fault = new Faultcode();
                    fault.id = int.Parse(r["id"].ToString());
                    fault.faultcode = r["faultcode"].ToString();
                    fault.descripcion = r["descripcion"].ToString();
                    list.Add(fault);
                }
            }
        }

        /// <summary>
        /// Devuelve la descripcion del codigo faultcode solicitado
        /// </summary>
        public static string Description(string faultcode)
        {
            Faultcode faultcodeList = list.Find(obj => obj.faultcode == faultcode);
            string textFaultcode = "Sin definir";
            if (faultcodeList != null)
            {
                textFaultcode = faultcodeList.descripcion;
            }

            return textFaultcode;
        }

        public static int Total()
        {
            return list.Count();
        }
    }
}
