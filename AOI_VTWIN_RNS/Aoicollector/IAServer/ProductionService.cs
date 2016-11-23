
using CollectorPackage.Aoicollector.IAServer.Mapper;
using Newtonsoft.Json;
using System;

namespace CollectorPackage.Aoicollector
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
                string path = string.Format("{0}/aoicollector/prodinfo/{1}", apiUrl, aoibarcode);
                
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
