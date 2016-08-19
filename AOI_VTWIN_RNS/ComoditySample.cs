using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AOI_VTWIN_RNS
{
    public class Auto
    {
        public string modelo { get; set; }
        public int precio { get; set; }
    }


    public class AutoEventArgs : EventArgs
    {
        public Auto auto { get; set; }

        public AutoEventArgs(Auto auto)
        {
            this.auto = auto;
        }
    }

    public class AutoRepository
    {
        public IEnumerable<Auto> GetAutos()
        {
            return new List<Auto>()
            {
                new Auto(){modelo = "Subaru", precio= 180000},
                new Auto(){modelo = "Tico", precio= 48000},
                new Auto(){modelo = "Astra", precio= 14000}
            };
        }
    }

    public class AutoMonitor
    {
        private Auto _auto;
        public event EventHandler<AutoEventArgs> AutoChanged;

        public Auto auto
        {
            get
            {
                return _auto;
            }
            set
            {
                _auto = value;
                this.OnAutoChange(new AutoEventArgs(_auto));
            }
        }

        protected virtual void OnAutoChange(AutoEventArgs e)
        {
            if (AutoChanged != null)
            {
                AutoChanged(this, e);
            }
        }
    }

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
