using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data;

using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Vts500.Controller;

namespace AOI_VTWIN_RNS.Aoicollector.Vts500
{
    public class VTS500 : Vts500Inspection
    {
        public VTS500(RichTextBox log, TabControl tabControl, ProgressBar progress)
        {
            Prepare("VTS500", "V", log, tabControl, progress, WorkerStart);
        }

        private void WorkerStart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            aoiLog.log("WorkerStart()");
 
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
            aoiLog.log("StartInspection()");

            bool OracleSuccess = false;
            
            OracleController oc = new OracleController(this);
            OracleSuccess = oc.GetMachines();
            
            if (OracleSuccess)
            {
                aoiLog.info("Comenzando analisis de inspecciones");

                // Lista de maquinas VTWIN
                IEnumerable<Machine> oracleMachines = Machine.list.Where(obj => obj.tipo == aoiConfig.machineNameKey);

                // Generacion de tabs segun las maquinas descargadas
                foreach (Machine inspMachine in oracleMachines.OrderBy(o => int.Parse(o.linea)))
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
                    if (Config.isByPassMode(inspMachine.linea))
                    {
                        // SKIP MACHINE
                        aoiLog.warning("Maquina en ByPass: " + inspMachine.linea);
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
            if (Config.isByPassMode(inspMachine.linea))
            {
                inspMachine.LogBroadcast("warning",
                    string.Format("{0} en ByPass {1}", inspMachine.smd, inspMachine.line_barcode)
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
