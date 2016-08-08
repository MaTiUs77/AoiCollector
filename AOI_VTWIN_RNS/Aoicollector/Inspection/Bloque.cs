using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AOI_VTWIN_RNS.Src.Util.Crypt;
using AOI_VTWIN_RNS.Src.Config;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
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
