using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models
{
    public class Sfcs
    {
        [JsonProperty("regex")]
        public string regex { get; set; }

        [JsonProperty("op")]
        public string op { get; set; }

        [JsonProperty("line_id")]
        public int line_id { get; set; }

        [JsonProperty("puesto")]
        public string puesto { get; set; }

        [JsonProperty("puesto_id")]
        public int puesto_id { get; set; }

        [JsonProperty("modelo_id")]
        public int modelo_id { get; set; }

        [JsonProperty("declara")]
        public string declara { get; set; }

        [JsonProperty("list")]
        public List<Sfcs> list { get; set; }
    }
}
