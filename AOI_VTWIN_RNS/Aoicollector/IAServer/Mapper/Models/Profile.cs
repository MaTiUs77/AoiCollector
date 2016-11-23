using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models
{
    public class Profile
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("nombre")]
        public string nombre { get; set; }

        [JsonProperty("apellido")]
        public string apellido { get; set; }

        [JsonProperty("dni")]
        public string dni { get; set; }

        [JsonProperty("legajo")]
        public string legajo { get; set; }

        [JsonProperty("user_id")]
        public string user_id { get; set; }
    }
}
