using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using CollectorPackage.Aoicollector.Core;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Vtwin.Controller;
using System.Data;

namespace CollectorPackage.Aoicollector.Vtwin
{
    public class VTWIN : VtwinInspection
    {
        public VTWIN(RichTextBox log, TabControl tabControl, ProgressBar progress)
        {
            Prepare("VTWIN", "W", log, tabControl, progress, WorkerStart);
        }

        private void WorkerStart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            CheckPcbFiles();
            try
            {
                if (aoiReady)
                {
                    StartInspection();
                } else
                {
                    aoiLog.warning("WorkerStart() => CheckPcbFiles() => no finalizo correctamente");
                }
            }
            catch (Exception ex)
            {
                aoiLog.stack("WorkerStart()", this, ex);
            }
        }

        private void StartInspection()
        {
            bool OracleSuccess = false;

            OracleController oc = new OracleController(this);
            OracleSuccess = oc.GetMachines();

            if (OracleSuccess)
            {
                aoiLog.debug("Comenzando analisis de inspecciones");

                // Lista de maquinas VTWIN
                IEnumerable<Machine> oracleMachines = Machine.list.Where(obj => obj.tipo == aoiConfig.machineNameKey);
                List<Machine> endInspect = new List<Machine>();

                // Generacion de tabs segun las maquinas descargadas
                foreach (Machine inspMachine in oracleMachines.OrderBy(o => o.nroLinea))
                {
                    DynamicTab(inspMachine);
                }

                try
                {
                    HandlePendientInspection();
                }
                catch (Exception ex)
                {
                    aoiLog.stack(ex.Message, this, ex);
                } 
                       
                #region HandleInspection
                foreach (Machine inspMachine in oracleMachines)
                {
                    // Algunas maquinas las inspecciono al final, porque son lentas para procesar
                    if (Config.isEndInspect(inspMachine))
                    {
                        endInspect.Add(inspMachine);
                    }
                    else
                    {
                        TryInspectionProccess(inspMachine);
                    }


                } // end foreach
                #endregion

                #region MAQUINAS DE PROCESO LENTO AL FINAL
                foreach (Machine inspMachine in endInspect)
                {
                    TryInspectionProccess(inspMachine);
                }
                #endregion
            }
        }    
        
        private void TryInspectionProccess(Machine inspMachine)
        {
            if (Config.isByPassMode(inspMachine))
            {
                inspMachine.LogBroadcast("warning",
                    string.Format("{0} {1} | En ByPass / Se detiene el proceso de inspeccion", inspMachine.maquina, inspMachine.smd)
                );
            }
            else
            {
                try
                {
                    HandleInspection(inspMachine);
                }
                catch (Exception ex)
                {
                    inspMachine.log.stack(ex.Message, this, ex);
                }
            }

            inspMachine.LogBroadcast("verbose", 
                string.Format("TryInspectionProccess({0}) Completo", inspMachine.smd)
            );
        }
    }
}
