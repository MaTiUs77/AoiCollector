using CollectorPackage.Aoicollector.IAServer.Mapper.Models;
using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper
{
    public class ProdInfoMapper
    {
        [JsonProperty("error")]
        public string error { get; set; }

        [JsonProperty("produccion")]
        public Produccion produccion { get; set; }
    }
}
