using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollectorPackage.Aoicollector.Inspection
{
    public class Revision
    {
        public string barcode;

        public string revisionAoi;
        public string revisionIns;
        public int totalErrores = 0;
        public int totalErroresReales = 0;
        public int totalErroresFalsos = 0;

        public bool isInvalid;
        public string tipoBarcode;

        public List<Detail> detailList = new List<Detail>();

        public void BarcodeValidate()
        {
            BarcodeValidator validator = new BarcodeValidator(barcode);
            isInvalid = validator.isInvalid;
            tipoBarcode = validator.tipoBarcode;
        }

        public void MakeRevision()
        {
            IEnumerable<Detail> detListReal = detailList.Where(x=>x.estado.Equals("REAL"));
            IEnumerable<Detail> detListFalso = detailList.Where(obj => obj.estado == "FALSO");
            IEnumerable<Detail> detListPendiente = detailList.Where(obj => obj.estado == "PENDIENTE");

            totalErroresFalsos = detListFalso.Count() + detListPendiente.Count();
            totalErroresReales = detListReal.Count();
            totalErrores = totalErroresFalsos + totalErroresReales;

            if (totalErrores == 0)
            {
                revisionAoi = "OK";
                revisionIns = "OK";
            }
            else
            {
                revisionAoi = "NG";
                revisionIns = "NG";

                if (totalErroresReales == 0)
                {
                    revisionIns = "OK";
                }

                if (totalErroresFalsos == 0)
                {
                    revisionAoi = "OK";
                }
            }
        }
    }
}
