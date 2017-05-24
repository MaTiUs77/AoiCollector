using CollectorPackage.Aoicollector.IAServer.Mapper.Models;
using CollectorPackage.Aoicollector.IAServer.Mapper.Models.Cogiscan;
using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper
{
    public class QueryItemMapper
    {
        [JsonProperty("error")]
        public QueryItemAttributes attributes { get; set; }

        [JsonProperty("Product")]
        public QueryItemProduct Product { get; set; }
    }
}
