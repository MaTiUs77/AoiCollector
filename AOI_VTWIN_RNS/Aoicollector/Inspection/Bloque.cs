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
