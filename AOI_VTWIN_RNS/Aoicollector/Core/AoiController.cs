using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

using CollectorPackage.Src.Config;
using CollectorPackage.Src.Util.Network;
using CollectorPackage.Src.Database;
using CollectorPackage.Aoicollector.Core;
using CollectorPackage.Aoicollector.Inspection.Model;
using System.Drawing;
using System.Threading.Tasks;

namespace CollectorPackage.Aoicollector
{
    public class AoiController
    {
        public bool aoiReady = false;
        public OracleConnector oracle = new OracleConnector();
        public SqlServerConnector sqlserver = new SqlServerConnector();

        public Config aoiConfig      { get; set; }
        public Worker aoiWorker { get; set; }
        public RichLog aoiLog { get; set; }
        public TabControl aoiTabControl { get; set; }
        public List<RichLog> aoiTabLogList { get; set; }

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
            if(oracle.host != null)
            {
                aoiLog.info("------------------- Oracle config -----------------------");
                aoiLog.info("+ Server: " + oracle.host);
                aoiLog.info("+ Port: " + oracle.port);
                aoiLog.info("+ User: " + oracle.user);
                aoiLog.info("+ Pass: " + oracle.pass);
                aoiLog.info("+ Service: " + oracle.service);
                aoiLog.info("---------------------------------------------------------");
            }
        }

        private void LoadSqlserver()
        {
            sqlserver.LoadConfig(aoiConfig.machineType);
            if (sqlserver.host != null)
            {
                aoiLog.info("------------------- SqlServer config -----------------------");
                aoiLog.info("+ Server: " + sqlserver.host);
                aoiLog.info("+ Port: " + sqlserver.port);
                aoiLog.info("+ User: " + sqlserver.user);
                aoiLog.info("+ Pass: " + sqlserver.pass);
                aoiLog.info("+ Database: " + sqlserver.database);
                aoiLog.info("---------------------------------------------------------");
            }
        }

        public void LoadDB()
        {
            string dbType = AppConfig.Read(aoiConfig.machineType, "db_type");
            switch(dbType)
            {
                case "sqlserver":
                    LoadSqlserver();
                    break;
                case "oracle":
                    LoadOracle();
                    break;
            }
        }

        public void Prepare(string machineType, string machineNameKey, RichTextBox logRichTextBox, TabControl tabControl, ProgressBar progress, DoWorkEventHandler WorkerStart)
        {
            aoiLog = new RichLog(logRichTextBox);
            aoiTabControl = tabControl;

            LoadConfig(machineType, machineNameKey);
            LoadWorker(progress, WorkerStart);
            LoadDB();
            UseCredential();

            aoiLog.verbose("Prepare() de " + machineType + " completo");
        }

        public void TotalMachines()
        {
            IEnumerable<Machine> machines = Machine.list.Where(obj => obj.tipo == aoiConfig.machineNameKey);
            aoiLog.notify(string.Format("Maquinas: {0} ", machines.Count()));
        }

        public async void Start(bool forceStart = false)
        {
            if (Config.dbDownloadComplete)
            {
                aoiLog.verbose("Iniciando operaciones");
                aoiWorker.StartOperation(forceStart);
            }
            else
            {
                aoiLog.warning("No se pudo descargar informacion del servidor.");
                aoiLog.verbose("Re intentando conexion...");

                bool downloaded = await Task.Run(() => 
                    Config.dbDownload()
                );

                if (downloaded)
                {
                    aoiLog.verbose("Iniciando operaciones");
                    aoiWorker.StartOperation(forceStart); 
                }
            }
        }

        public void Stop()
        {
            aoiLog.verbose("Deteniendo operaciones");
            aoiWorker.StopTimer();
        }

        public bool UseCredential()
        {
            bool complete = false;
            if (Convert.ToBoolean(AppConfig.Read(aoiConfig.machineType, "usar_credencial")))
            {
                aoiLog.debug("Ejecutando credencial: " + AppConfig.Read(aoiConfig.machineType, "server"));
                try
                {
                    Network.ConnectCredential(aoiConfig.machineType);                    
                    aoiLog.debug("Credencial ejecutada");
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

            //aoiLog.verbose("CheckPcbFiles() " + aoiConfig.dataProgPath);

            if (UseCredential())
            {
                aoiLog.debug("Verificando cambios en PCB Files");
                try
                {
                    PcbData pcbData = new PcbData(this);
                    bool reload = pcbData.VerifyPcbFiles();
                    if (reload)
                    {
                        aoiLog.notify("Actualizando lista de PCB Files en memoria");
                        PcbInfo.Download(aoiConfig.machineNameKey);
                    }
                    aoiLog.debug("Verificacion de PCB Files completa");
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

        /// <summary>
        /// Agrega un TAB dinamico, con su respectivo box de logeo
        /// </summary>
        /// <param name="inspMachine"></param>
        public void DynamicTab(Machine inspMachine)
        {
            MethodInvoker makeDyndamicTab = new MethodInvoker(() =>
            {
                string id = "dyn" + inspMachine.line_barcode;
                string smd = inspMachine.smd;

                RichLog rlog = aoiTabLogList.Find(o => o.id.Equals(id));
                if (rlog == null)
                {
                    TabPage dynTab = new TabPage();
                    aoiTabControl.Controls.Add(dynTab);
                    dynTab.Name = "tab_" + id;
                    dynTab.Text = smd;
                    dynTab.UseVisualStyleBackColor = true;

                    RichTextBox richTextBoxDyn = new RichTextBox();

                    dynTab.Controls.Add(richTextBoxDyn);

                    richTextBoxDyn.BackColor = Color.Black;
                    richTextBoxDyn.Cursor = Cursors.IBeam;
                    richTextBoxDyn.Dock = DockStyle.Fill;
                    richTextBoxDyn.Font = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                    richTextBoxDyn.ForeColor = Color.White;
                    richTextBoxDyn.Name = dynTab.Name + "_rich";
                    richTextBoxDyn.ReadOnly = true;
                    richTextBoxDyn.Text = "";
               
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
    }
}
