using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models.Cogiscan
{
    public class QueryItemProductAttributes
    {
        [JsonProperty("batchId")]
        public string batchId { get; set; }

        [JsonProperty("partNumber")]
        public string partNumber { get; set; }

        [JsonProperty("revision")]
        public string revision { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("releaseRouteName")]
        public string releaseRouteName { get; set; }

        [JsonProperty("lastStatusChangeTmst")]
        public string lastStatusChangeTmst { get; set; }

        [JsonProperty("operation")]
        public string operation { get; set; }

        [JsonProperty("defectDisposition")]
        public string defectDisposition { get; set; }

        [JsonProperty("quantity")]
        public string quantity { get; set; }
    }
}
