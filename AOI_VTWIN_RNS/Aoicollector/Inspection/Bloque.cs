using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CollectorPackage.Src.Util.Crypt;
using CollectorPackage.Src.Config;

namespace CollectorPackage.Aoicollector.Inspection
{
    public class Bloque: Revision
    {
        public int bloqueId;

        public Bloque(string barcode)
        {
            this.barcode = barcode;
            BarcodeValidate();
        }
    }
}
