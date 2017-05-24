using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using CollectorPackage.Src.Config;
using CollectorPackage.Src.Database;

namespace CollectorPackage
{
    public partial class Mysql_FormConfiguration : Form
    {
        public Mysql_FormConfiguration()
        {
            InitializeComponent();
        }

        private void confDB_Load(object sender, EventArgs e)
        {
            MySqlConnector iaserverDatabase = new MySqlConnector();
            iaserverDatabase.LoadConfig("IASERVER");

            MyServer.Text = iaserverDatabase.host;
//            MyPort.Text = iaserverDatabase.port;
            MyUser.Text = iaserverDatabase.user;
            MyPass.Text = iaserverDatabase.pass;
            MyDatabase.Text = iaserverDatabase.database;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool close = true;

            close = ValidarMysql();

            MySqlConnector iaserverDatabase = new MySqlConnector();
            iaserverDatabase.LoadConfig("IASERVER");

            if (close)
            {
                Close();
            }
        }

        private bool ValidarMysql()
        {
            bool valid = true;
            if (!MyUser.Text.Trim().Equals(""))
            {
                AppConfig.Save("IASERVER", "db_user", MyUser.Text.Trim());
            }
            else
            {
                MessageBox.Show("Complete el campo Usuario");
                valid = false;
            }

            if (!MyPass.Text.Trim().Equals(""))
            {
                AppConfig.Save("IASERVER", "db_pass", MyPass.Text.Trim());
            }
            else
            {
                MessageBox.Show("Complete el campo Clave");
                valid = false;
            }

            if (!MyServer.Text.Trim().Equals(""))
            {
                AppConfig.Save("IASERVER", "db_host", MyServer.Text.Trim());
            }
            else
            {
                MessageBox.Show("Complete el campo Servidor");
                valid = false;
            }

            if (!MyDatabase.Text.Trim().Equals(""))
            {
                AppConfig.Save("IASERVER", "db_database", MyDatabase.Text.Trim());
            }
            else
            {
                MessageBox.Show("Complete el campo Database");
                valid = false;
            }
            return valid;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
