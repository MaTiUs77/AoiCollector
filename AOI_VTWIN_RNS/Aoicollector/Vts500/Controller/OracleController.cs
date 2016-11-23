using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Inspection;
using System.Data;
using CollectorPackage.Src.Database;
using CollectorPackage.Aoicollector.Core;

namespace CollectorPackage.Aoicollector.Vts500.Controller
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
            aoi.aoiLog.info("[ORACLE] Descargando maquinas");

            try
            {
                GetAllMachines();
                aoi.aoiLog.debug("+ Descarga completa");
                success = true;
            }
            catch (Exception ex)
            {
                aoi.aoiLog.stack(ex.Message, this, ex);
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
                    aoi.aoiLog.warning("No se encuentra agregada a la base de datos la maquina: " + name);
                }
            }
        }
    }
}
