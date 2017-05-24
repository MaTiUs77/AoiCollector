using CollectorPackage.Src.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AOICollector.Forms
{
    public partial class Redis_FormConfiguration : Form
    {
        public Redis_FormConfiguration()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            AppConfig.Save("REDIS", "host", host.Text.Trim());

            this.Close();
        }

        private void Redis_FormConfiguration_Load(object sender, EventArgs e)
        {
            host.Text = AppConfig.Read("REDIS", "host");
        }
    }
}
