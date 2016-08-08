using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AOI_VTWIN_RNS.Src.Util.Crypt;
using AOI_VTWIN_RNS.Src.Config;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
{
    public class BarcodeValidator
    {
        public string regexInvalidCodeFilter = @"^[a-zA-Z0-9_-]*$";
        public string regexDefault = @"^\d+$"; // AppConfig.Read("SERVICE", "expresion_regular").ToString();

        public bool isInvalid;
        public string barcode;
        public string tipoBarcode;

        public BarcodeValidator(string barcode)
        {
            this.barcode = barcode;
            validateBarcode();
        }

        public void validateBarcode()
        {
            // Solucion a problema de caracteres invalidos en barcode
            var regexItem = new Regex(regexInvalidCodeFilter);
            if (!regexItem.IsMatch(barcode))
            {
                barcode = "_invalid_code_" + Crypt.Md5(barcode);
                isInvalid = true;
            }

            if (BarcodeMatchExpresion(barcode))
            {
                tipoBarcode = "E";
            }
            else
            {
                tipoBarcode = "V";
            }
        }

        private bool BarcodeMatchExpresion(string barcode)
        {
            // Etiquetas tipo: 00000123244
            Regex regex = new Regex(regexDefault);
            if (regex.IsMatch(barcode))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
