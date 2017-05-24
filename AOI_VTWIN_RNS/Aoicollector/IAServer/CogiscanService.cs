
using CollectorPackage.Aoicollector.IAServer.Mapper;
using Newtonsoft.Json;
using System;

namespace CollectorPackage.Aoicollector
{
    public class CogiscanService : Api
    {
        public QueryItemMapper result;

        public QueryItemMapper GetQueryItem(string barcode)
        {
            result = new QueryItemMapper();
            hasResponse = false;

            try
            {
                string path = string.Format("{0}/cogiscan/queryItem/{1}", apiUrl, barcode);
                
                string jsonData = Consume(path);
                hasResponse = true;
                result = JsonConvert.DeserializeObject<QueryItemMapper>(jsonData);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            return result;
        }
    }
}
