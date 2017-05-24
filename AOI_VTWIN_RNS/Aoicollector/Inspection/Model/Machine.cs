using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using CollectorPackage.Src.Database;
using CollectorPackage.Aoicollector.Core;
using System.Diagnostics;
using CollectorPackage.Src.Util.Convertion;

namespace CollectorPackage.Aoicollector.Inspection.Model
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
        public int nroLinea;
        public string smd;
        public string tipo;
        public string ultima_inspeccion;
        public string ping;
        public string line_barcode;
        public bool active = true;

        public ProductionService prodService;

        public ProductionService GetProductionInfoFromIAServer()
        {
            /*
            LogBroadcast("verbose",
               string.Format("+ Verificando informacion de produccion desde IAServer ({0})", line_barcode)
            );
           */

            Stopwatch sw = Stopwatch.StartNew();
            prodService = new ProductionService();
            prodService.GetProdInfo(line_barcode);
            sw.Stop();

            LogBroadcast("success",
               string.Format("API GetProductionInfoFromIAServer - Tiempo de respuesta: (ms) {0} ",
               (long)sw.ElapsedMilliseconds)
            );

            if (prodService.error == null)
            {
                // Existe configuracion de rutas? ya sea sfcs o iaserver?
                if (prodService.routeMode!=null)
                {
                    #region LOG Produccion Info                   
                    LogBroadcast("verbose",
                        string.Format(
                            "============== PRODINFO {0} ==============\n " +
                            "Activa: {1} \n " +
                            "Semielaborado: {2} \n " +
                            "Producir: {3} \n " +
                            "Ruta modo: {4} \n " +
                            "Ruta puesto: {5} \n " +
                            "Ruta Declara: {6} \n " +
                            "IASERVER: \t {7}% Inspecciones: {8} / Restantes: {9} \n ",
                            prodService.result.produccion.op,
                            prodService.result.produccion.wip.active,
                            prodService.result.produccion.wip.wip_ot.codigo_producto,
                            prodService.result.produccion.wip.wip_ot.start_quantity,
                            prodService.routeMode,
                            prodService.routeName,
                            prodService.routeDeclare,
                            prodService.result.produccion.smt.porcentaje,
                            prodService.result.produccion.smt.prod_aoi,
                            prodService.result.produccion.smt.restantes
                        )
                    );

                    if(prodService.routeDeclare)
                    {
                        LogBroadcast("verbose",
                            string.Format(
                                "EBS: \t\t {0}% Declaradas: {1} / Restantes: {2} \n ",
                                prodService.result.produccion.wip.wip_ot.porcentaje,
                                prodService.result.produccion.wip.wip_ot.quantity_completed,
                                prodService.result.produccion.wip.wip_ot.restante
                            )
                        );
                    }
                    #endregion
                }
                else
                {
                    LogBroadcast("warning", "No existe configuracion de ruta de produccion");
                }
            }
            else
            {
                log.stack(
                    string.Format("Stack Error en la verificacion de produccion desde IAServer ({0})", line_barcode
                ), this, prodService.error);
            }
                
            return prodService;
        }

        public void Ping()
        {
            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            string query = @"UPDATE  `aoidata`.`maquina` SET  `ping` =  NOW() WHERE  `id` = " + mysql_id + " LIMIT 1;";
            DataTable sp = sql.Query(query);
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
                m.ultima_inspeccion_iaserver,
                m.active,
                m.ping
            from
                aoidata.maquina as m
            left join aoidata.produccion p ON m.id = p.id_maquina
            ";

            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            DataTable dt = sql.Query(query);
            if (sql.rows)
            {
                foreach (DataRow r in dt.Rows)
                {
                    Machine mac = new Machine();
                    mac.mysql_id = int.Parse(r["id"].ToString());
                    mac.maquina = r["maquina"].ToString();
                    mac.nroLinea = int.Parse(r["linea"].ToString());
                    mac.smd = "SMD-"+mac.nroLinea;
                    mac.tipo = r["tipo"].ToString();
                    mac.ultima_inspeccion = r["ultima_inspeccion"].ToString();
                    mac.line_barcode = r["barcode"].ToString();
                    mac.active = bool.Parse(r["active"].ToString());
                    mac.ping = r["ping"].ToString();
                    list.Add(mac);
                }
            }
        }

        public static Machine findByCode(string aoicode)
        {
            return list.Find(obj => obj.maquina == aoicode);
        }

        public static void UpdateInspectionDate(int id, DateTime custom_date)
        {
            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            string query = @"CALL sp_updateInspeccionMaquina(" + id + ",'" + custom_date.ToString("yyyy-MM-dd HH:mm:ss") + "');";
            DataTable sp = sql.Query(query);
            if (sql.rows)
            {
                string ultima_inspeccion = sp.Rows[0]["ultima_inspeccion"].ToString();

                Machine update = list.Find(obj => obj.mysql_id == id);
                update.ultima_inspeccion = ultima_inspeccion;
            }
        }

        public static void UpdateInspectionDate(int id)
        {
            MySqlConnector sql = new MySqlConnector();
            sql.LoadConfig("IASERVER");
            string query = @"CALL sp_updateInspeccionMaquina(" + id + ", NOW());";
            DataTable sp = sql.Query(query);
            if (sql.rows)
            {
                string ultima_inspeccion = sp.Rows[0]["ultima_inspeccion"].ToString();

                Machine update = list.Find(obj => obj.mysql_id == id);
                update.ultima_inspeccion = ultima_inspeccion;
            }
        }

        public static int Total()
        {
            return list.Count();
        }

        public void LogBroadcast(string mode, string msg)
        {
            if (mode.Equals("debug")) {
                if(glog.loglevel < 1) {
                    glog.putLog(msg, mode, true);
                    log.putLog(msg, mode, true);
                }
            } else
            {
                glog.putLog(msg, mode, true);
                log.putLog(msg, mode, true);
            }
        }
    }
}
