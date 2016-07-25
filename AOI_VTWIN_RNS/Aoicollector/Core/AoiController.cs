using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text.RegularExpressions;

using AOI_VTWIN_RNS.Src.Config;
using AOI_VTWIN_RNS.Src.Util.Network;
using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Aoicollector.Inspection;

namespace AOI_VTWIN_RNS.Aoicollector
{
    public class AoiController : InspectionController
    {
        public Config aoiConfig      { get; set; }
        public Worker aoiWorker { get; set; }
        public Log aoiLog { get; set; }

        public OracleConnector oracle = new OracleConnector();

        public bool aoiReady = false;

        public AoiController()
        {
            aoiConfig = new Config();
            aoiLog = new Log();
            aoiWorker = new Worker(this);
        }

        public void LoadConfig(string machineType, string machineNameKey)
        {
            aoiConfig.machineType = machineType;
            aoiConfig.machineNameKey = machineNameKey;
            aoiConfig.inspectionCsvPath = AppConfig.Read(machineType, "csvPath");
            aoiConfig.xmlExportPath = AppConfig.Read(machineType, "xmlExport");
            aoiConfig.dataProgPath = AppConfig.Read(machineType, "dataProg");
            aoiConfig.intervalo = int.Parse(AppConfig.Read(machineType, "intervalo"));
        }
        private void LoadWorker(ProgressBar progress, DoWorkEventHandler WorkerStart)
        {
            aoiWorker.timerInterval = aoiConfig.intervalo;
            aoiWorker.progressBar = progress;
            aoiWorker.WorkerStart = new DoWorkEventHandler(WorkerStart);
        }
        private void LoadOracle()
        {
            oracle.LoadConfig(aoiConfig.machineType);
            if(oracle.server != null)
            {
                aoiLog.Area("------------------- Oracle config -----------------------");
                aoiLog.Area("+ Server: " + oracle.server);
                aoiLog.Area("+ Service: " + oracle.service);
                aoiLog.Area("+ Port: " + oracle.port);
                aoiLog.Area("+ User: " + oracle.user);
                aoiLog.Area("+ Pass: " + oracle.pass);
                aoiLog.Area("---------------------------------------------------------");
            }
        }

        public void Prepare(string machineType, string machineNameKey, ListBox logArea, ProgressBar progress, DoWorkEventHandler WorkerStart)
        {
            aoiLog.logArea = logArea;

            LoadConfig(machineType, machineNameKey);
            LoadWorker(progress, WorkerStart);
            LoadOracle();
            UseCredential();

            aoiLog.Area("Prepare() de " + machineType + " completo");
        }
        public void Start(bool forceStart = false)
        {
            if (Config.dbDownloadComplete)
            {
                aoiLog.Area("Iniciando operaciones");
                aoiWorker.StartOperation(forceStart);
            }
            else
            {
                aoiLog.Area("No se pudo descargar informacion del servidor.");
                Config.dbDownload();
                aoiLog.Area("Re intentando conexion, estado: "+Config.dbDownloadComplete.ToString());
            }

        }
        public void Stop()
        {
            aoiLog.Area("Deteniendo operaciones");
            aoiWorker.StopTimer();
        }
        public bool UseCredential()
        {
            bool complete = false;
            if (Convert.ToBoolean(AppConfig.Read(aoiConfig.machineType, "usar_credencial")))
            {
                aoiLog.Area("Conectando a: " + AppConfig.Read(aoiConfig.machineType, "server"));
                try
                {
                    Network.ConnectCredential(aoiConfig.machineType);
                    aoiLog.Area("+ Credencial ejecutada.");
                    complete = true;

                }
                catch (Exception ex)
                {
                    complete = false;
                    aoiLog.Area("+ No fue posible ejecutar la credencial. " + ex.Message, "error");
                    Log.Stack(this, ex);
                }
            }
            else
            {
                complete = true;
            }

            aoiReady = complete;
            return complete;
        }

        /// <summary>
        /// Verifica si hay cambios en los archivos PCB de AOI
        /// </summary>
        public bool CheckPcbFiles()
        {
            bool complete = false;

            if (UseCredential())
            {
                aoiLog.Area("Verificando cambios de PCB Files en: " + aoiConfig.dataProgPath);
                try
                {
                    PcbData pcbData = new PcbData(this);
                    bool reload = pcbData.VerifyPcbFiles();
                    if (reload)
                    {
                        aoiLog.Area("- Recargando lista de PCB Files en memoria");
                        PcbInfo.Download(aoiConfig.machineNameKey);
                    }
                    aoiLog.Area("+ Verificacion de PCB Files completa");
                    complete = true;
                }
                catch (Exception ex)
                {
                    aoiLog.Area(ex.Message, "error");
                    Log.Stack(this, ex);
                    complete = false;
                }
            }

            return complete;
        }

        // WORKER ALIAS
        public void ProgressTotal(int total)
        {
            aoiWorker.SetProgressTotal(total);
        }
        public void ProgressInc(int current_num)
        {
            aoiWorker.SetProgressWorking(current_num);
        }

        public void ResetInspection()
        {
            inspectionObj = new InspectionObject();
        }        
    }
}
