using System;
using System.Windows.Forms;

using AOI_VTWIN_RNS.Src.Config;
using AOI_VTWIN_RNS.Aoicollector;

namespace AOI_VTWIN_RNS
{
    public partial class Zenith_FormConfiguration : Form
    {
        public AoiController aoi;
        public Zenith_FormConfiguration(AoiController _aoi)
        {
            InitializeComponent();
            this.aoi = _aoi;
        }

        private void confVTWIN_Load(object sender, EventArgs e)
        {
            numIntervalo.Minimum = 1;

            txtXML.Text = AppConfig.Read(aoi.aoiConfig.machineType, "xmlExport");
            numIntervalo.Value = int.Parse(AppConfig.Read("ZENITH", "intervalo"));

            txtHost.Text = AppConfig.Read(aoi.aoiConfig.machineType, "db_host");
            txtUser.Text = AppConfig.Read(aoi.aoiConfig.machineType, "db_user");
            txtPass.Text = AppConfig.Read(aoi.aoiConfig.machineType, "db_pass");
            txtPort.Text = AppConfig.Read(aoi.aoiConfig.machineType, "db_port");
        }

        private void btnXML_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fol = new FolderBrowserDialog();
            fol.ShowDialog();
            if (fol.SelectedPath != "")
            {
                txtXML.Text = fol.SelectedPath;
            }
        }

        private bool Validate(string variable, string value, string msg = null)
        {
            bool valid = true;
            if (!value.Trim().Equals(""))
            {
                AppConfig.Save(aoi.aoiConfig.machineType, variable, value.Trim());
            }
            else
            {
                if (msg != null)
                {
                    MessageBox.Show(msg);
                }
                valid = false;
            }
            return valid;
        }

        private bool SaveConfig()
        {
            bool save = true;

            Validate("xmlExport", txtXML.Text);

            if (numIntervalo.Value > 0)
            {
                AppConfig.Save(aoi.aoiConfig.machineType, "intervalo", numIntervalo.Value.ToString());
            }

            save = Validate("db_host", txtHost.Text, "Complete el campo Servidor");
            save = Validate("db_port", txtPort.Text, "Complete el campo Puerto");
            save = Validate("db_user", txtUser.Text, "Complete el campo Usuario");
            save = Validate("db_pass", txtPass.Text, "Complete el campo Clave");

            // Cargo la configuracion de los campos nuevamente
            aoi.LoadConfig(aoi.aoiConfig.machineType, aoi.aoiConfig.machineNameKey);

            return save;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {         
            if (SaveConfig())
            {
                this.Close();
            }
        }
    }
}