using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
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
                DataTable dt = sql.Select(query);
                if (sql.rows)
                {
                    DataRow r = dt.Rows[0];
                    idPanel = int.Parse(r["id"].ToString());
                }
            }
            catch (Exception ex)
            {
                isStackError = true;
                Log.Stack(this, ex);
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
                    DataTable dt = sql.Select(query);
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
                Log.Stack(this, ex);
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
                    DataTable dt = sql.Select(query);
                }
            }
            catch (Exception ex)
            {
                isStackError = true;
                Log.Stack(this, ex);
            }
        }
    }
}
