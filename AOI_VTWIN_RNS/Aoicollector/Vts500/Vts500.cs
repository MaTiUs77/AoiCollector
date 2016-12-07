using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data;

using CollectorPackage.Aoicollector.Core;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Vts500.Controller;

namespace CollectorPackage.Aoicollector.Vts500
{
    public class VTS500 : Vts500Inspection
    {
        public VTS500(RichTextBox log, TabControl tabControl, ProgressBar progress)
        {
            Prepare("VTS500", "V", log, tabControl, progress, WorkerStart);
        }

        private void WorkerStart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            aoiLog.debug("WorkerStart()");
 
            try
            {
                StartInspection();
            }
            catch (Exception ex)
            {
                Log.Stack("WorkerStart()",this, ex);
            }
        }

        private void StartInspection()
        {
            aoiLog.debug("StartInspection()");

            bool OracleSuccess = false;
            
            OracleController oc = new OracleController(this);
            OracleSuccess = oc.GetMachines();
            
            if (OracleSuccess)
            {
                aoiLog.info("Comenzando analisis de inspecciones");

                // Lista de maquinas VTWIN
                IEnumerable<Machine> oracleMachines = Machine.list.Where(obj => obj.tipo == aoiConfig.machineNameKey);

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
                    // Filtro maquinas en ByPass
                    if (Config.isByPassMode(inspMachine))
                    {
                        // SKIP MACHINE
                        aoiLog.warning(
                            string.Format("{0} {1} | En ByPass / Se detiene el proceso de inspeccion", inspMachine.maquina, inspMachine.smd)
                        );
                    }
                    else
                    {
                        TryInspectionProccess(inspMachine);
                    }
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
