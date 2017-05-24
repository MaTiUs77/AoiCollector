using System.Text.RegularExpressions;
using CollectorPackage.Src.Util.Crypt;

namespace CollectorPackage.Aoicollector.Inspection
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
            ValidateBarcode();
        }

        public void ValidateBarcode()
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

        public bool ValidateBarcodeWithRegex(string barcode,string regexExpression)
        {
            Regex regex = new Regex(regexExpression);
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
