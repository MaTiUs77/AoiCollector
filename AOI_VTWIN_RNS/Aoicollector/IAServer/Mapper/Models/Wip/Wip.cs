using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models.WipModel
{
    public class Wip
    {
        [JsonProperty("active")]
        public bool active { get; set; }

        [JsonProperty("wip_ot")]
        public WipOt wip_ot { get; set; }
    }
}
