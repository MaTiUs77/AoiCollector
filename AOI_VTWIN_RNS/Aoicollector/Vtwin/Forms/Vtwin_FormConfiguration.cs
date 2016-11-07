using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using AOI_VTWIN_RNS.Src.Config;
using AOI_VTWIN_RNS.Aoicollector;

namespace AOI_VTWIN_RNS
{
    public partial class Vtwin_FormConfiguration : Form
    {
        public AoiController aoi;
        public Vtwin_FormConfiguration(AoiController _aoi)
        {
            InitializeComponent();
            this.aoi = _aoi;
        }

        private void confVTWIN_Load(object sender, EventArgs e)
        {
            numIntervalo.Minimum = 1;

            txtXML.Text = AppConfig.Read(aoi.aoiConfig.machineType, "xmlExport");
            txtPCB.Text = AppConfig.Read(aoi.aoiConfig.machineType, "dataProg");
            numIntervalo.Value = int.Parse(AppConfig.Read("VTWIN", "intervalo"));

            txtServer.Text = AppConfig.Read(aoi.aoiConfig.machineType, "server");
            txtUser.Text = AppConfig.Read(aoi.aoiConfig.machineType, "user");
            txtPass.Text = AppConfig.Read(aoi.aoiConfig.machineType, "pass");

            txtOracleServer.Text = AppConfig.Read(aoi.aoiConfig.machineType, "db_host");
            txtOracleUser.Text = AppConfig.Read(aoi.aoiConfig.machineType, "db_user");
            txtOraclePass.Text = AppConfig.Read(aoi.aoiConfig.machineType, "db_pass");
            txtOraclePort.Text = AppConfig.Read(aoi.aoiConfig.machineType, "db_port");
            txtOracleService.Text = AppConfig.Read(aoi.aoiConfig.machineType, "db_service");
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

        private void btnPCB_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fol = new FolderBrowserDialog();
            fol.ShowDialog();
            if (fol.SelectedPath != "")
            {
                txtPCB.Text = fol.SelectedPath;
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
            Validate("dataProg", txtPCB.Text);

            if (numIntervalo.Value > 0)
            {
                AppConfig.Save(aoi.aoiConfig.machineType, "intervalo", numIntervalo.Value.ToString());
            }

            save = Validate("server", txtServer.Text, "Complete el campo Servidor");
            save = Validate("user", txtUser.Text, "Complete el campo Usuario");
            save = Validate("pass", txtPass.Text, "Complete el campo Clave");

            save = Validate("db_host", txtOracleServer.Text, "Complete el campo Servidor");
            save = Validate("db_port", txtOraclePort.Text, "Complete el campo Puerto");
            save = Validate("db_user", txtOracleUser.Text, "Complete el campo Usuario");
            save = Validate("db_pass", txtOraclePass.Text, "Complete el campo Clave");
            save = Validate("db_service", txtOracleService.Text, "Complete el campo Servicio");

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