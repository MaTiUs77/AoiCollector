using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CollectorPackage
{
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
}
