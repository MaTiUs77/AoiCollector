using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models
{
    public class Smt
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("op")]
        public string op { get; set; }

        [JsonProperty("modelo")]
        public string modelo { get; set; }

        [JsonProperty("lote")]
        public string lote { get; set; }

        [JsonProperty("panel")]
        public string panel { get; set; }

        [JsonProperty("semielaborado")]
        public string semielaborado { get; set; }

        [JsonProperty("prod_aoi")]
        public int? prod_aoi { get; set; }

        [JsonProperty("prod_man")]
        public int? prod_man { get; set; }

        [JsonProperty("qty")]
        public int? qty { get; set; }

        [JsonProperty("registros")]
        public int? registros { get; set; }

        [JsonProperty("porcentaje")]
        public float? porcentaje { get; set; }

        [JsonProperty("restantes")]
        public int? restantes { get; set; }

        public bool isBottom()
        {
            Regex regex = new Regex(@"^OP-\d+-B$");
            if (regex.IsMatch(op))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
