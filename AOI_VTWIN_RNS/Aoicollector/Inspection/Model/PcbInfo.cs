using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Data;

using CollectorPackage.Src.Database;

namespace CollectorPackage.Aoicollector.Inspection.Model
{
    public class PcbInfo
    {
        public static List<PcbInfo> list = new List<PcbInfo>();

        public int id = 0;
        public string nombre = "";
        public string programa = "";
        public int bloques;
        public string segmentos = "";
        public string tipoMaquina = "";
        public string hash = "";
        public string fechaModificacion = "";
        public string libreria = "";
        public int etiquetas = 0;
        public int etiquetasPcbId = 0;
        public int secundaria = 0;

        public static string Hash(string _filePath)
        {
            byte[] computedHash = new MD5CryptoServiceProvider().ComputeHash(File.ReadAllBytes(_filePath));
            var sBuilder = new StringBuilder();
            foreach (byte b in computedHash)
            {
                sBuilder.Append(b.ToString("x2").ToLower());
            }
            return sBuilder.ToString();
        }
        
        public static void Download(string _tipoMaquina = "ALL")
        {
            string filtroMaquina = "";

            switch (_tipoMaquina)
            {
                case "W":
                    // Reseteo solo programas VTWIN.
                    list.RemoveAll(o => o.tipoMaquina == "W");
                    filtroMaquina = " where tipo_maquina = 'W' ";
                    break;
                case "Z":
                    // Reseteo solo programas ZENITH.
                    list.RemoveAll(o => o.tipoMaquina == "Z");
                    filtroMaquina = " where tipo_maquina = 'Z' ";
                    break;
                case "R":
                    // Reseteo solo programas RNS.
                    list.RemoveAll(o => o.tipoMaquina == "R");
                    filtroMaquina = " where tipo_maquina = 'R' ";
                    break;
                case "ALL":
                    // Reseteo resultados SQL anteriores.
                    list = new List<PcbInfo>();
                    break;
            }

            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            DataTable query = sql.Query("select id,nombre,programa,bloques,segmentos,tipo_maquina,hash,DATE_FORMAT(fecha_modificacion,'%Y-%m-%d %H:%i:%s') as fecha_modificacion,libreria,etiquetas,secundaria from aoidata.pcb_data " + filtroMaquina);
            if (sql.rows)
            {
                foreach (DataRow r in query.Rows)
                {
                    PcbInfo n = new PcbInfo();
                    n.id = int.Parse(r["id"].ToString());
                    n.programa = r["programa"].ToString();
                    n.nombre = r["nombre"].ToString();
                    n.bloques = int.Parse(r["bloques"].ToString());
                    n.segmentos = r["segmentos"].ToString();
                    n.tipoMaquina = r["tipo_maquina"].ToString();
                    n.hash = r["hash"].ToString();
                    n.fechaModificacion = r["fecha_modificacion"].ToString();
                    string _etiquetas = r["etiquetas"].ToString();
                    try
                    {
                        n.etiquetas = int.Parse(_etiquetas);
                    } catch(Exception ex)
                    {
                        n.etiquetas = 0;
                    }

                    string sec = r["secundaria"].ToString();
                    if (sec.ToLower().Equals("false"))
                    {
                        n.secundaria = 0;
                    }

                    if (sec.ToLower().Equals("true"))
                    {
                        n.secundaria = 1;
                    }
                    n.libreria = r["secundaria"].ToString();
                    list.Add(n);
                }
            }
        }

        public static bool Update(PcbInfo pcb)
        {
            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            string query = "UPDATE aoidata.pcb_data SET bloques = '" + pcb.bloques + "',segmentos = '" + pcb.segmentos + "',hash = '" + pcb.hash + "',fecha_modificacion = '" + pcb.fechaModificacion + "' ,libreria = '" + pcb.libreria + "',etiquetas = '" + pcb.etiquetas + "', secundaria = '" + pcb.secundaria+ "' WHERE id = '" + pcb.id + "' limit 1";
            bool rs = sql.NonQuery(query);
            if (rs)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int Insert(PcbInfo pcb)
        {
            string nombre = pcb.programa.Split('.').FirstOrDefault();

            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            string query = @"CALL sp_addPcbData('" + nombre + "', '" + pcb.programa + "', '" + pcb.bloques + "','" + pcb.segmentos + "', '" + pcb.tipoMaquina + "', '" + pcb.hash + "', '" + pcb.fechaModificacion + "');";
            DataTable sp = sql.Query(query);
            if (sql.rows)
            {
                pcb.id = int.Parse(sp.Rows[0]["id"].ToString());
            }

            // Retorno ID, 0 si no pudo insertar.
            return pcb.id;
        }

        public static int Total()
        {
            return list.Count();
        }

    }
}
