using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CollectorPackage
{
    public class SubaruObserver
    {
        public SubaruObserver(AutoMonitor monitor)
        {
            monitor.AutoChanged += monitor_CommodityChange;
        }

        void monitor_CommodityChange(object sender, AutoEventArgs e)
        {
            CheckFilter(e.auto);
        }

        private void CheckFilter(Auto commodity)
        {
            Thread.Sleep(6000);
            if (commodity.modelo == "Subaru" && commodity.precio > 100000)
            {
                System.Windows.Forms.MessageBox.Show(string.Format("El subaru ta caro {0}", commodity.precio));
            }
        }
    }
}
