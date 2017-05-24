using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models
{
    public class Route
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("op")]
        public string op { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("declare")]
        public string declare { get; set; }

        [JsonProperty("regex")]
        public string regex { get; set; }

        [JsonProperty("qty_etiquetas")]
        public string qty_etiquetas { get; set; }

        [JsonProperty("qty_bloques")]
        public string qty_bloques { get; set; }

        [JsonProperty("is_secundaria")]
        public string is_secundaria { get; set; }
    }
}
