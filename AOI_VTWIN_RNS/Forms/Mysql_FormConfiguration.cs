using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using AOI_VTWIN_RNS.Src.Config;
using AOI_VTWIN_RNS.Src.Database;

namespace AOI_VTWIN_RNS
{
    public partial class Mysql_FormConfiguration : Form
    {
        public Mysql_FormConfiguration()
        {
            InitializeComponent();
        }

        private void confDB_Load(object sender, EventArgs e)
        {
            MyUser.Text = AppConfig.Read("MYSQL", "user");
            MyPass.Text = AppConfig.Read("MYSQL", "pass");
            MyServer.Text = AppConfig.Read("MYSQL", "server");
            MyDatabase.Text = AppConfig.Read("MYSQL", "database");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool close = true;

            close = ValidarMysql();

            MySqlConnector.LoadConfig();

            if (close)
            {
                this.Close();
            }
        }

        private bool ValidarMysql()
        {
            bool valid = true;
            if (!MyUser.Text.Trim().Equals(""))
            {
                AppConfig.Save("MYSQL", "user", MyUser.Text.Trim());
            }
            else
            {
                MessageBox.Show("Complete el campo Usuario");
                valid = false;
            }

            if (!MyPass.Text.Trim().Equals(""))
            {
                AppConfig.Save("MYSQL", "pass", MyPass.Text.Trim());
            }
            else
            {
                MessageBox.Show("Complete el campo Clave");
                valid = false;
            }

            if (!MyServer.Text.Trim().Equals(""))
            {
                AppConfig.Save("MYSQL", "server", MyServer.Text.Trim());
            }
            else
            {
                MessageBox.Show("Complete el campo Servidor");
                valid = false;
            }

            if (!MyDatabase.Text.Trim().Equals(""))
            {
                AppConfig.Save("MYSQL", "database", MyDatabase.Text.Trim());
            }
            else
            {
                MessageBox.Show("Complete el campo Database");
                valid = false;
            }
            return valid;
        }
    }
}
