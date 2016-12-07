using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using CollectorPackage.Aoicollector.Core;
using CollectorPackage.Aoicollector.Inspection.Model;
using System.Data;
using CollectorPackage.Aoicollector.Zenith.Controller;

namespace CollectorPackage.Aoicollector.Zenith
{
    public class ZENITH : ZenithInspection
    {
        public ZENITH(RichTextBox log, TabControl tabControl, ProgressBar progress)
        {
            Prepare("ZENITH", "Z", log, tabControl, progress, WorkerStart);
        }

        private void WorkerStart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            StartInspection();
        }

        private void StartInspection()
        {
            SqlServerController sqlctrl = new SqlServerController(this);

            // Lista de maquinas Zenith
            IEnumerable<Machine> machines = Machine.list.Where(obj => obj.tipo == aoiConfig.machineNameKey);

            // Generacion de tabs segun las maquinas descargadas
            foreach (Machine inspMachine in machines.OrderBy(o => o.nroLinea))
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

            foreach (Machine inspMachine in machines)
            {
                TryInspectionProccess(inspMachine);
            }
        }    
        
        private void TryInspectionProccess(Machine inspMachine)
        {
            inspMachine.LogBroadcast("debug", 
                string.Format("============================ {0} ==== {1} ===============================",
                    inspMachine.maquina, 
                    inspMachine.smd
                ));

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
        }
    }
}
