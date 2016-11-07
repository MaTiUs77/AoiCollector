using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;

using AOI_VTWIN_RNS.Src.Util.Files;

namespace AOI_VTWIN_RNS.Aoicollector.Rns
{
    public class RNS : RnsInspection
    {
        public RNS(RichTextBox log, TabControl tabControl, ProgressBar progress)
        {
            Prepare("RNS", "R", log, tabControl, progress, WorkerStart);
        }

        private void WorkerStart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            aoiLog.verbose("WorkerStart()");
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
            aoiLog.verbose("StartInspection() => Localizando CSV de Inspeccion");

            // Obtengo archivos CSV en InspectionFolder
            IOrderedEnumerable<FileInfo> csv = FilesHandler.GetFiles("*", aoiConfig.inspectionCsvPath);
            int totalCsv = csv.Count();

            aoiWorker.SetProgressTotal(totalCsv);

            if (totalCsv > 0)
            {
                int file_count = 0;
                aoiLog.info("CSV encontrados: " + totalCsv);

                foreach (FileInfo file in csv)
                {
                    file_count++;
                    HandleInspection(file);

                    aoiWorker.SetProgressWorking(file_count);
                }

                aoiLog.info("No hay mas CSV");
            }
            else
            {
                aoiLog.info("No se encontraron inspecciones.");
            }
        }
    }
}
