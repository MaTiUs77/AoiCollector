using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models.Cogiscan
{
    public class ResponseEndSmt
    {
        [JsonProperty("exceptionType")]
        public string exceptionType { get; set; }
    }
}
