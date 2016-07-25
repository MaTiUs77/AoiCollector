using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Inspection;
using System.Data;
using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS.Aoicollector.Vts500.Controller
{
    public class OracleController : OracleQuery
    {
        public AoiController aoi;

        public OracleController(AoiController _aoi) 
        {
            aoi = _aoi;
        }

        public bool GetMachines()
        {
            bool success = false;
            aoi.aoiLog.Area("[ORACLE] Descargando maquinas");

            try
            {
                GetAllMachines();
                aoi.aoiLog.Area("+ Descarga completa", "info");
                success = true;
            }
            catch (Exception ex)
            {
                aoi.aoiLog.Area(ex.Message, "error");
                Log.Stack(this, ex);
            }

            return success;
        }

        /// <summary>
        ///  Obtiene lista de id,maquina de base de datos oracle y las adhiere a Machine.list
        /// </summary>
        private void GetAllMachines()
        {

            string query = OracleQuery.ListMachines();
            DataTable dt = aoi.oracle.Query(query);
            foreach (DataRow dtr in dt.Rows)
            {
                string name = dtr["SYS_MACHINE_NAME"].ToString();

                Machine im = Machine.list.Find(obj => obj.maquina == name);
                if (im == null)
                {
                    aoi.aoiLog.Area("No se encuentra agregada a la base de datos la maquina: " + name, "atencion");
                }
            }
        }
    }
}
