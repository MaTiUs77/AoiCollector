using CollectorPackage.Aoicollector.IAServer.Mapper;
using Newtonsoft.Json;
using System;

namespace CollectorPackage.Aoicollector.IAServer
{
    public class PanelService : Api
    {
        public PanelInfoMapper result;

        public PanelInfoMapper GetPanelInfo(string panelBarcode,bool verifyDeclared = true)
        {
            result = new PanelInfoMapper();
            hasResponse = false;

            try
            {
                string path = string.Format("{0}/api/aoicollector/placa/{1}", apiUrl, panelBarcode);
                if(verifyDeclared)
                {
                    path = string.Format("{0}/api/declared", path);
                }

                string jsonData = Consume(path);
                hasResponse = true;
                result = JsonConvert.DeserializeObject<PanelInfoMapper>(jsonData);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            return result;
        }
    }
}
