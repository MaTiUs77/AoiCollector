using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Data;

using AOI_VTWIN_RNS.Src.Util.Files;
using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Vts500.Controller;

namespace AOI_VTWIN_RNS.Aoicollector.Vts500
{
    public class VTS500 : Vts500Inspection
    {
        public VTS500(ListBox log, ProgressBar progress)
        {
            Prepare("VTS500", "V", log, progress, WorkerStart);
        }

        private void WorkerStart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            aoiLog.Area("WorkerStart()");
 
            try
            {
                if (aoiReady)
                {
                    StartInspection();
                }
            }
            catch (Exception ex)
            {
                Log.Stack(this, ex);
            }
        }

        private void StartInspection()
        {
            aoiLog.Area("StartInspection()");

            bool OracleSuccess = false;
            
            OracleController oc = new OracleController(this);
            OracleSuccess = oc.GetMachines();
            
            if (OracleSuccess)
            {
                aoiLog.Area("Comenzando analisis de inspecciones");

                try
                {
                    HandlePendientInspection();
                }
                catch (Exception ex)
                {
                    aoiLog.Area(ex.Message, "error");
                    Log.Stack(this, ex);
                }

                IEnumerable<Machine> oracleMachines = Machine.list.Where(obj => obj.tipo == aoiConfig.machineNameKey);
                foreach (Machine inspMachine in oracleMachines)
                {
                    // Filtro maquinas en ByPass
                    if (Config.isByPassMode(inspMachine.linea))
                    {
                        // SKIP MACHINE
                        aoiLog.Area("+ Maquina en ByPass: " + inspMachine.linea);
                    }
                    else
                    {
                        try
                        {
                            HandleInspection(inspMachine);
                        }
                        catch (Exception ex)
                        {
                            aoiLog.Area(ex.Message, "error");
                            Log.Stack(this, ex);
                        }
                    }
                }
            }
        }
    }
}
