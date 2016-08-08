using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Inspection;
using System.Data;
using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS.Aoicollector.Vtwin.Controller
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
            aoi.aoiLog.info("Descargando maquinas desde Oracle");

            try
            {
                GetAllMachines();
                aoi.aoiLog.log("Descarga completa");
                success = true;
            }
            catch (Exception ex)
            {
                aoi.aoiLog.stack(ex.Message,this, ex);
            }

            return success;
        }

        /// <summary>
        ///  Obtiene lista de id,maquina de base de datos oracle y las adhiere a Machine.list_machine
        /// </summary>
        private void GetAllMachines()
        {
            string query = OracleQuery.ListMachines();
            DataTable dt = aoi.oracle.Query(query);
            foreach (DataRow dtr in dt.Rows)
            {
                int oracle_id = int.Parse(dtr["MACHINE_ID"].ToString());
                string name = dtr["MACHINE_NAME"].ToString();

                Machine im = Machine.list.Find(obj => obj.maquina == name);
                if (im == null)
                {
                    aoi.aoiLog.warning("No se encuentra agregada a la base de datos la maquina: " + name);
                }
                else
                {
                    im.oracle_id = oracle_id;
                }
            }
        }
    }
}
