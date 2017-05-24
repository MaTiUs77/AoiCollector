using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models.Cogiscan
{
    public class QueryItemProduct
    {
        [JsonProperty("attributes")]
        public QueryItemProductAttributes attributes { get; set; }
    }
}
