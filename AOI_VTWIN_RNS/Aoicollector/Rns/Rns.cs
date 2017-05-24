using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;

using CollectorPackage.Src.Util.Files;
using CollectorPackage.Aoicollector.Core;
using Newtonsoft.Json.Linq;

namespace CollectorPackage.Aoicollector.Rns
{
    public class RNS : RnsInspection
    {
        public RNS(RichTextBox log, TabControl tabControl, ProgressBar progress)
        {
            Prepare("RNS", "R", log, tabControl, progress, WorkerStart);
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
            aoiLog.verbose("Localizando CSV de Inspeccion");

            // Obtengo archivos CSV en InspectionFolder
            IOrderedEnumerable<FileInfo> csv = FilesHandler.GetFiles("*", aoiConfig.inspectionCsvPath);
            int totalCsv = csv.Count();

            aoiWorker.SetProgressTotal(totalCsv);           

            if (totalCsv > 0)
            {
                int file_count = 0;

                foreach (FileInfo file in csv)
                {
                    file_count++;

                    #region REDIS
                        // Dato a enviar
                        JObject json = new JObject();
                        json["mode"] = "runtime";
                        json["tipo"] = aoiConfig.machineType;
                        json["current"] = file_count.ToString();
                        json["total"] = totalCsv.ToString();
                        // Enviar al canal 
                        Realtime.send(json.ToString());
                    #endregion

                    aoiLog.info("---------------------------------------------");
                    aoiLog.info(" Procesando " + file_count + " / " + totalCsv);
                    aoiLog.info("---------------------------------------------");

                    HandleInspection(file);

                    aoiWorker.SetProgressWorking(file_count);
                }

                aoiLog.info("No hay mas CSV");
            }
            else
            {
                #region REDIS
                    // Dato a enviar
                    JObject json = new JObject();
                    json["mode"] = "runtime";
                    json["tipo"] = aoiConfig.machineType;
                    json["current"] = 0;
                    json["total"] = 0;
                    // Enviar al canal 
                    Realtime.send(json.ToString());
                #endregion

                aoiLog.info("No se encontraron inspecciones.");
            }
        }
    }
}
