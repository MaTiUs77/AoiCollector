using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models
{
    public class Inspector
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("created_at")]
        public string created_at { get; set; }

        [JsonProperty("updated_at")]
        public string updated_at { get; set; }

        [JsonProperty("fullname")]
        public string fullname { get; set; }

        [JsonProperty("profile")]
        public Profile profile { get; set; }
    }
}
