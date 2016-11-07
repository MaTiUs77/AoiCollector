using AOI_VTWIN_RNS.Aoicollector.IAServer.Mapper.Models;
using Newtonsoft.Json;

namespace AOI_VTWIN_RNS.Aoicollector.IAServer.Mapper
{
    public class ProdInfoMapper
    {
        [JsonProperty("error")]
        public string error { get; set; }

        [JsonProperty("produccion")]
        public Produccion produccion { get; set; }
    }
}
