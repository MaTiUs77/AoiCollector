using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using System.Data;
using AOI_VTWIN_RNS.Aoicollector.Zenith.Controller;

namespace AOI_VTWIN_RNS.Aoicollector.Zenith
{
    public class ZENITH : ZenithInspection
    {
        public ZENITH(RichTextBox log, TabControl tabControl, ProgressBar progress)
        {
            Prepare("ZENITH", "Z", log, tabControl, progress, WorkerStart);
        }

        private void WorkerStart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            aoiLog.verbose("WorkerStart()");

            StartInspection();
        }

        private void StartInspection()
        {
            aoiLog.verbose("StartInspection()");

            SqlServerController sqlctrl = new SqlServerController(this);

            aoiLog.info("Comenzando analisis de inspecciones");

            // Lista de maquinas Zenith
            IEnumerable<Machine> machines = Machine.list.Where(obj => obj.tipo == aoiConfig.machineNameKey);

            // Generacion de tabs segun las maquinas descargadas
            foreach (Machine inspMachine in machines.OrderBy(o => int.Parse(o.linea)))
            {
                DynamicTab(inspMachine);
            }

            try
            {
                //HandlePendientInspection();
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
