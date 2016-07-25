using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.IO;

using AOI_VTWIN_RNS.Src.Util.Files;
using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Vtwin.Controller;
using System.Data;

namespace AOI_VTWIN_RNS.Aoicollector.Vtwin
{
    public class VTWIN : VtwinInspection
    {
        public VTWIN(ListBox log, ProgressBar progress)
        {
            Prepare("VTWIN", "W", log, progress, WorkerStart);
        }

        private void WorkerStart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            aoiLog.Area("WorkerStart()");
            CheckPcbFiles();

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

                // Lista de maquinas VTWIN
                IEnumerable<Machine> oracleMachines = Machine.list.Where(obj => obj.tipo == aoiConfig.machineNameKey);
                List<Machine> endInspect = new List<Machine>();

                try
                {
                    HandlePendientInspection();
                }
                catch (Exception ex)
                {
                    aoiLog.Area(ex.Message, "error");
                    Log.Stack(this, ex);
                } 
                       
                #region HandleInspection
                foreach (Machine inspMachine in oracleMachines)
                {
                    // Algunas maquinas las inspecciono al final, porque son lentas para procesar
                    if (Config.isEndInspect(inspMachine.linea))
                    {
                        endInspect.Add(inspMachine);
                    }
                    else
                    {
                        // Filtro maquinas en ByPass
                        if (Config.isByPassMode(inspMachine.linea))
                        {
                            // SKIP MACHINE
                            aoiLog.Area("+ Maquina en ByPass: " + inspMachine.linea);
                        } else 
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
                } // end foreach
                #endregion

                #region MAQUINAS DE PROCESO LENTO AL FINAL
                foreach (Machine inspMachine in endInspect)
                {
                    if (Config.isByPassMode(inspMachine.linea))
                    {
                        // SKIP MACHINE
//                        aoiLog.Area("+ Maquina en ByPass: " + inspMachine.linea);
                    } else 
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
                #endregion
            }
        }        
    }
}
