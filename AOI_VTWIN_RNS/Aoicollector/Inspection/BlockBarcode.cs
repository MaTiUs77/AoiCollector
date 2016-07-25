using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AOI_VTWIN_RNS.Src.Util.Crypt;
using AOI_VTWIN_RNS.Src.Config;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
{
    public class BlockBarcode
    {
        public int bloqueId;
        public string barcode;
        public string tipoBarcode;

        public string revision_aoi = "NG";
        public string revision_ins = "NG";
        public int total_errores_falsos = 0;
        public int total_errores_reales = 0;
        public int total_errores = 0;

        public List<Detail> detailList = new List<Detail>();

        public void validateBarcode()
        {
            // Solucion a problema de caracteres invalidos en barcode
            var regexItem = new Regex("^[a-zA-Z0-9_-]*$");
            if (!regexItem.IsMatch(barcode))
            {
                barcode = "_invalid_code_" + Crypt.Md5(barcode);
            }

            if (BlockBarcode.BarcodeMatchExpresion(barcode))
            {
                tipoBarcode = "E";
            }
            else
            {
                tipoBarcode = "V";
            }
        }

        public static void validateBarcode(InspectionObject inspectionObj)
        {
            // Solucion a problema de caracteres invalidos en barcode
            var regexItem = new Regex("^[a-zA-Z0-9_-]*$");
            if (!regexItem.IsMatch(inspectionObj.panelBarcode))
            {
                inspectionObj.panelBarcode = "_invalid_code_" + Crypt.Md5(inspectionObj.panelBarcode);
            }

            if (BlockBarcode.BarcodeMatchExpresion(inspectionObj.panelBarcode))
            {
                inspectionObj.tipoPanelBarcode = "E";
            }
            else
            {
                inspectionObj.tipoPanelBarcode = "V";
            }
        }

        public void processDetail() 
        { 
            IEnumerable<Detail> blockReal = detailList.Where(obj => obj.estado == "REAL");
            IEnumerable<Detail> blockFalso = detailList.Where(obj => obj.estado == "FALSO");
            IEnumerable<Detail> blockPendiente = detailList.Where(obj => obj.estado == "PENDIENTE");

            total_errores_falsos = blockFalso.Count() + blockPendiente.Count();
            total_errores_reales = blockReal.Count();
            total_errores = total_errores_falsos + total_errores_reales;

            if (total_errores == 0)
            {
                revision_aoi = "OK";
                revision_ins = "OK";
            }
            else
            {
                if (total_errores_reales == 0)
                {
                    revision_ins = "OK";
                }
            }
        }

        /// <summary>
        /// El Barcode es valido? (segun la configuracion de etiqueta stantard)
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns>bool</returns>
        public static bool BarcodeMatchExpresion(string barcode)
        {
            // Etiqueta limitada a 10 caracteres numericos
            // string numeric_10 = @"^\d{10}$";

            // Etiquetas tipo: 00000123244
            string numeric = AppConfig.Read("SERVICE", "expresion_regular").ToString(); //@"^\d+$";
            Regex regex = new Regex(numeric);
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
