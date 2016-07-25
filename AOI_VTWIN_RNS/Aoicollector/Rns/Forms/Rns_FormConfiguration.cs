using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Configuration;

using AOI_VTWIN_RNS.Src.Config;
using AOI_VTWIN_RNS.Aoicollector;

namespace AOI_VTWIN_RNS
{
    public partial class Rns_FormConfiguration : Form
    {
        public AoiController aoi;
        public Rns_FormConfiguration(AoiController _aoi)
        {
            InitializeComponent();
            this.aoi = _aoi;
        }

        private void confRNS_Load(object sender, EventArgs e)
        {
            numIntervalo.Minimum = 1;

            txtXML.Text = AppConfig.Read(aoi.aoiConfig.machineType,"xmlExport");
            txtPCB.Text = AppConfig.Read(aoi.aoiConfig.machineType, "dataProg");
            txtCSV.Text = AppConfig.Read(aoi.aoiConfig.machineType, "csvPath");
            numIntervalo.Value = int.Parse(AppConfig.Read(aoi.aoiConfig.machineType, "intervalo"));

            txtServer.Text = AppConfig.Read(aoi.aoiConfig.machineType, "server");
            txtUser.Text = AppConfig.Read(aoi.aoiConfig.machineType, "user");
            txtPass.Text = AppConfig.Read(aoi.aoiConfig.machineType, "pass");
        }

        private void btnCSV_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fol = new FolderBrowserDialog();
            fol.ShowDialog();
            if (fol.SelectedPath != "")
            {
                txtCSV.Text = fol.SelectedPath;
            }
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

        private bool Validate(string variable, string value, string msg)
        {
            bool valid = true;
            if (!value.Trim().Equals(""))
            {
                AppConfig.Save(aoi.aoiConfig.machineType, variable, value.Trim());
            }
            else
            {
                MessageBox.Show(msg);
                valid = false;
            }
            return valid;
        }

        private bool SaveConfig()
        {
            bool save = true;

            save = Validate("xmlExport", txtXML.Text, "Complete el campo XML");
            save = Validate("dataProg", txtPCB.Text, "Complete el campo PCB");
            save = Validate("csvPath", txtCSV.Text, "Complete el campo CSV");

            if (numIntervalo.Value > 0)
            {
                AppConfig.Save(aoi.aoiConfig.machineType, "intervalo", numIntervalo.Value.ToString());
            }

            save = Validate("server", txtServer.Text, "Complete el campo Servidor");
            save = Validate("user", txtUser.Text, "Complete el campo Usuario");
            save = Validate("pass", txtPass.Text, "Complete el campo Clave");

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
