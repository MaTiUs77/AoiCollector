using CollectorPackage.Aoicollector.IAServer.Mapper.Models.Cogiscan;
using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper
{
    public class AoicollectorPassed
    {
        [JsonProperty("error")]
        public ResponseEndSmt attributes { get; set; }
    }
}
