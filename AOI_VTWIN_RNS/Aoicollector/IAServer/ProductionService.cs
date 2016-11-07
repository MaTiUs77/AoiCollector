
using AOI_VTWIN_RNS.Aoicollector.IAServer.Mapper;
using Newtonsoft.Json;
using System;

namespace AOI_VTWIN_RNS.Aoicollector
{
    public class ProductionService : Api
    {
        public ProdInfoMapper result;

        public ProdInfoMapper GetProdInfo(string aoibarcode)
        {
            result = new ProdInfoMapper();
            hasResponse = false;

            try
            {
                string path = apiUrl + "prodinfo/" + aoibarcode;
                //string path = "http://localhost:8080/aoibarcode/VTRNS6061";
                
                string jsonData = Consume(path);
                hasResponse = true;
                result = JsonConvert.DeserializeObject<ProdInfoMapper>(jsonData);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            return result;
        }
    }
}
