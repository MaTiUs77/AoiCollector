using System;

namespace CollectorPackage.Aoicollector.Inspection
{
    public class Declarar : Api
    {
        public void withPanelBarcode(string panelBarcode)
        {
            hasResponse = false;

            try
            {
                string path = string.Format("{0}/api/aoicollector/declarar/{1}", apiUrl, panelBarcode);

                string jsonData = Consume(path);
                hasResponse = true;
            }
            catch (Exception ex)
            {
                error = ex;
            }
        }
    }
}
