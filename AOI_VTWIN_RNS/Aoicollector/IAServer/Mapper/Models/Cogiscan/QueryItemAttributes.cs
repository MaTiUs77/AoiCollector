using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models.Cogiscan
{
    public class QueryItemAttributes
    {
        [JsonProperty("exceptionType")]
        public string exceptionType { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }

        [JsonProperty("itemId")]
        public string itemId { get; set; }

        [JsonProperty("itemKey")]
        public string itemKey { get; set; }

        [JsonProperty("itemType")]
        public string itemType { get; set; }

        [JsonProperty("itemTypeClass")]
        public string itemTypeClass { get; set; }

        [JsonProperty("quarantineLocked")]
        public string quarantineLocked { get; set; }
    }
}
