using System;
using System.Data;

using CollectorPackage.Src.Database;
using CollectorPackage.Aoicollector.Core;

namespace CollectorPackage.Aoicollector.Inspection
{
    public class History
    {
        public bool isStackError = false;

        public int idPanel = 0;
        public int idBloque = 0;       
        
        /// <summary>
        /// Envia al historial la inspeccion realizada del panel
        /// </summary>
        /// <param name="save_id_panel"></param>
        public void SavePanel(int save_id_panel,string mode)
        {
            try
            {
                string query = "CALL aoidata.sp_insertHistoryPanel(" + save_id_panel + ", '" + mode + "')";
                MySqlConnector sql = new MySqlConnector();
                sql.LoadConfig("IASERVER");
                DataTable dt = sql.Query(query);
                if (sql.rows)
                {
                    DataRow r = dt.Rows[0];
                    idPanel = int.Parse(r["id"].ToString());
                }
            }
            catch (Exception ex)
            {
                isStackError = true;
                Log.Stack("HistorySavePanel()", this, ex);
            }
        }

        /// <summary>
        /// Envia al historial la inspeccion realizada del bloque
        /// </summary>
        /// <param name="save_id_bloque"></param>
        public void SaveBloque(int save_id_bloque)
        {
            try
            {
                if (idPanel > 0)
                {
                    string query = "CALL aoidata.sp_insertHistoryBlock(" + idPanel + "," + save_id_bloque + ")";
                    MySqlConnector sql = new MySqlConnector();
                    sql.LoadConfig("IASERVER");
                    DataTable dt = sql.Query(query);
                    if (sql.rows)
                    {
                        DataRow r = dt.Rows[0];
                        idBloque = int.Parse(r["id"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                isStackError = true;
                Log.Stack("HistorySaveBloque()", this, ex);
            }
        }

        /// <summary>
        /// Envia al historial los detalles de el ultimo bloque insertado 
        /// </summary>
        /// <param name="save_id_bloque"></param>
        public void SaveDetalle(int save_id_bloque)
        {
            try
            {
                if (idPanel > 0 && idBloque > 0)
                {
                    string query = "CALL aoidata.sp_insertHistoryDetail(" + save_id_bloque + ")";
                    MySqlConnector sql = new MySqlConnector();
                    sql.LoadConfig("IASERVER");
                    DataTable dt = sql.Query(query);
                }
            }
            catch (Exception ex)
            {
                isStackError = true;
                Log.Stack("HustorySaveDetalle()", this, ex);
            }
        }
    }
}
