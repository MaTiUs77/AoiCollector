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
using System.Drawing;
using System.Threading.Tasks;

namespace AOI_VTWIN_RNS.Aoicollector
{
    public class AoiController
    {
        public bool aoiReady = false;
        public OracleConnector oracle = new OracleConnector();
        public Config aoiConfig      { get; set; }
        public Worker aoiWorker { get; set; }
        public RichLog aoiLog { get; set; }
        public TabControl aoiTabControl { get; set; }
        public List<RichLog> aoiTabLogList { get; set; }
//        public InspectionController aoiInsp { get; set; }

        public AoiController()
        {
            aoiConfig = new Config();
            aoiTabLogList = new List<RichLog>();
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
                aoiLog.log("------------------- Oracle config -----------------------");
                aoiLog.log("+ Server: " + oracle.server);
                aoiLog.log("+ Service: " + oracle.service);
                aoiLog.log("+ Port: " + oracle.port);
                aoiLog.log("+ User: " + oracle.user);
                aoiLog.log("+ Pass: " + oracle.pass);
                aoiLog.log("---------------------------------------------------------");
            }
        }

        public void Prepare(string machineType, string machineNameKey, RichTextBox logRichTextBox, TabControl tabControl, ProgressBar progress, DoWorkEventHandler WorkerStart)
        {
            aoiLog = new RichLog(logRichTextBox);
            aoiTabControl = tabControl;

            LoadConfig(machineType, machineNameKey);
            LoadWorker(progress, WorkerStart);
            LoadOracle();
            UseCredential();

            aoiLog.log("Prepare() de " + machineType + " completo");
        }

        public async void Start(bool forceStart = false)
        {
            if (Config.dbDownloadComplete)
            {
                aoiLog.log("Iniciando operaciones");
                aoiWorker.StartOperation(forceStart);
            }
            else
            {
                aoiLog.warning("No se pudo descargar informacion del servidor.");
                aoiLog.verbose("Re intentando conexion...");

                bool downloaded = await Task.Run(() => Config.dbDownload());

                if (downloaded)
                {
                    aoiLog.log("Iniciando operaciones");
                    aoiWorker.StartOperation(forceStart); 
                }
            }
        }

        public void Stop()
        {
            aoiLog.log("Deteniendo operaciones");
            aoiWorker.StopTimer();
        }

        public bool UseCredential()
        {
            bool complete = false;
            if (Convert.ToBoolean(AppConfig.Read(aoiConfig.machineType, "usar_credencial")))
            {
                aoiLog.verbose("Ejecutando credencial: " + AppConfig.Read(aoiConfig.machineType, "server"));
                try
                {
                    Network.ConnectCredential(aoiConfig.machineType);
                    aoiLog.log("Credencial ejecutada.");
                    complete = true;
                }
                catch (Exception ex)
                {
                    complete = false;
                    aoiLog.stack("No fue posible ejecutar la credencial. " + ex.Message, this, ex);
                }
            }
            else
            {
                complete = true;
            }

            return complete;
        }

        /// <summary>
        /// Verifica si hay cambios en los archivos PCB de AOI
        /// </summary>
        public bool CheckPcbFiles()
        {
            bool complete = false;

            aoiLog.log("CheckPcbFiles() " + aoiConfig.dataProgPath);

            if (UseCredential())
            {
                aoiLog.verbose("Verificando cambios en PCB Files");
                try
                {
                    PcbData pcbData = new PcbData(this);
                    bool reload = pcbData.VerifyPcbFiles();
                    if (reload)
                    {
                        aoiLog.notify("Actualizando lista de PCB Files en memoria");
                        PcbInfo.Download(aoiConfig.machineNameKey);
                    }
                    aoiLog.log("Verificacion de PCB Files completa");
                    complete = true;
                }
                catch (Exception ex)
                {
                    aoiLog.stack(ex.Message, this, ex);
                    complete = false;
                }
            }

            aoiReady = complete;
            return complete;
        }

        public void DynamicTab(Machine inspMachine)
        {
            MethodInvoker makeDyndamicTab = new MethodInvoker(() =>
            {
                string id = "log" + inspMachine.linea;
                string smd = "SMD-" + inspMachine.linea;

                RichLog rlog = aoiTabLogList.Find(o => o.id.Equals(id));
                if (rlog == null)
                {
                    TabPage dynTab = new TabPage();
                    aoiTabControl.Controls.Add(dynTab);
                    dynTab.Name = "tab" + id;
                    dynTab.Text = smd;
                    dynTab.UseVisualStyleBackColor = true;

                    RichTextBox richTextBoxDyn = new RichTextBox();

                    dynTab.Controls.Add(richTextBoxDyn);

                    richTextBoxDyn.BackColor = Color.Black;
                    richTextBoxDyn.Cursor = Cursors.IBeam;
                    richTextBoxDyn.Dock = DockStyle.Fill;
                    richTextBoxDyn.Font = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                    richTextBoxDyn.ForeColor = Color.White;
                    richTextBoxDyn.Name = dynTab.Name + "richTextBox";
                    richTextBoxDyn.ReadOnly = true;
                    richTextBoxDyn.Text = "";
                    richTextBoxDyn.Name = "rich"+id;
               
                    RichLog addrlog = new RichLog(richTextBoxDyn);
                    addrlog.id = id;
                    addrlog.smd = smd;
                    aoiTabLogList.Add(addrlog);

                    inspMachine.log = addrlog;
                    inspMachine.glog = aoiLog;
                }

            });

            if (aoiTabControl.InvokeRequired)
            {
                aoiTabControl.Invoke(makeDyndamicTab);
            }
            else
            {
                makeDyndamicTab();
            }
        }

        //public void LogBroadcast(Machine inspMachine, string mode, string msg)
        //{
        //    inspMachine.log.putLog(aoiLog.putLog(msg, mode, false),mode,true);
        //}
    }
}
