using CollectorPackage.Aoicollector.IAServer.Mapper.Models;
using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper
{
    public class PanelInfoMapper
    {
        [JsonProperty("error")]
        public string error { get; set; }

        [JsonProperty("panel")]
        public Panel panel { get; set; }
    }
}
