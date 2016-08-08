using System;
using System.Windows.Forms;

using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Core;
using AOI_VTWIN_RNS.Aoicollector.Rns;
using AOI_VTWIN_RNS.Aoicollector.Vtwin;
using AOI_VTWIN_RNS.Aoicollector.Vts500;

namespace AOI_VTWIN_RNS
{
    public partial class MainForm : Form
    {
        public RNS rns;
        public VTWIN vtwin;
        public VTS500 vts500;

        #region Aplicacion inicial
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Carga configuracion de AppConfig
            MySqlConnector.LoadConfig();
            SqlServerConnector.LoadConfig();

            Log.system= new RichLog(systemRichTextBox);

            rns = new RNS(rnsLogGeneral, rnsTab,  prgRns);
            vtwin = new VTWIN(vtwinLogGeneral, vtwinTab,prgVtwin); 
            vts500 = new VTS500(vtsLogGeneral, vtsTab, prgVts500);


            if (Config.dbDownload())
            {
                // Envio la linea 14 a la ultima posicion de la lista de maquinas a inspeccionar
                // Por algun motivo demora mas que el resto en procesar las inspecciones
                Config.toEndInspect.Add(14);

                if (Config.isAutoStart())
                {
                    rns.Start(true);
                    vtwin.Start(true);
                    vts500.Start(true);
                }
                else
                {
                    /*
                     * En el siguiente update se deberian implementar eventos, esto permite a un sysadmin
                     * enviar un correo a una lista de contactos si el evento se disparo
                     */
                    //Evento.alerta("El modo 'autostart' no esta activo, el sistema no iniciara automaticamente los procesos!");
                }
            }
        }
        #endregion
        

        #region RNS MENU
        private void RnsMenu_Iniciar(object sender, EventArgs e)
        {
            rns.Start(true);
        }
        private void RnsMenu_Configurar(object sender, EventArgs e)
        {
            Rns_FormConfiguration form = new Rns_FormConfiguration(rns);
            form.ShowDialog();
        }
        private void RnsMenu_Detener(object sender, EventArgs e)
        {
            rns.Stop();
        }
        private void RnsMenu_clear(object sender, EventArgs e)
        {
            rns.aoiLog.reset();
        }
        #endregion

        #region VTWIN MENU
        private void VtwinMenu_Iniciar(object sender, EventArgs e)
        {
            vtwin.Start(true);
        }
        private void VtwinMenu_Configurar(object sender, EventArgs e)
        {
            Vtwin_FormConfiguration form = new Vtwin_FormConfiguration(vtwin);
            form.ShowDialog();
        }
        private void VtwinMenu_Detener(object sender, EventArgs e)
        {
            vtwin.Stop();
        }
        private void VtwinMenu_OracleQuery(object sender, EventArgs e)
        {
            Oracle_QueryClient form = new Oracle_QueryClient();
            form.oracle = vtwin.oracle;
            form.Show();
        }
        private void VtwinMenu_clear(object sender, EventArgs e)
        {
            vtwin.aoiLog.reset();
        }
        #endregion

        #region VTS500 MENU
        private void Vts500Menu_Iniciar(object sender, EventArgs e)
        {
            vts500.Start(true);
        }
        private void Vts500Menu_Configurar(object sender, EventArgs e)
        {
            Vts500_FormConfiguration form = new Vts500_FormConfiguration(vts500);
            form.ShowDialog();
        }
        private void Vts500Menu_Detener(object sender, EventArgs e)
        {
            vts500.Stop();
        }
        private void Vts500Menu_OracleQuery(object sender, EventArgs e)
        {
            Oracle_QueryClient form = new Oracle_QueryClient();
            form.oracle = vts500.oracle;
            form.Show();
        }
        private void Vts500Menu_clear(object sender, EventArgs e)
        {
            vts500.aoiLog.reset();
        }
        #endregion

        private void MysqlMenu_Configuration(object sender, EventArgs e)
        {
            Mysql_FormConfiguration form = new Mysql_FormConfiguration();
            form.Show();
        }

        private void byPassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LineFilter form = new LineFilter();
            form.ShowDialog();
        }

        private void ModeAutoScroll_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ModeAutoScroll_ToolStripMenuItem.Checked)
            {
                ModeAutoScroll_ToolStripMenuItem.Text = "Desactivado";
            }
            else
            {
                ModeAutoScroll_ToolStripMenuItem.Text = "Activado";
            }

            ModeAutoScroll_ToolStripMenuItem.Checked = !ModeAutoScroll_ToolStripMenuItem.Checked;
            Log.autoscroll = ModeAutoScroll_ToolStripMenuItem.Checked;
        }
    }
}
