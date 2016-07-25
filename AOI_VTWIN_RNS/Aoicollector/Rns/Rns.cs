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

namespace AOI_VTWIN_RNS.Aoicollector.Rns
{
    public class RNS : RnsInspection
    {
        public RNS(ListBox log, ProgressBar progress) 
        {
            Prepare("RNS", "R", log, progress, WorkerStart);
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
            aoiLog.Area("StartInspection() => Localizando CSV de Inspeccion");

            // Obtengo archivos CSV en InspectionFolder
            IOrderedEnumerable<FileInfo> csv = FilesHandler.GetFiles("*", aoiConfig.inspectionCsvPath);
            int totalCsv = csv.Count();

            ProgressTotal(totalCsv);

            if (totalCsv > 0)
            {
                int file_count = 0;
                aoiLog.Area("+ CSV encontrados: " + totalCsv);

                foreach (FileInfo file in csv)
                {
                    file_count++;

                    HandleInspection(file);

                    ProgressInc(file_count);
                }
                aoiLog.Area("+ No hay mas CSV");
            }
            else
            {
                aoiLog.Area("+ No se encontraron inspecciones.");
            }
        }
    }
}
