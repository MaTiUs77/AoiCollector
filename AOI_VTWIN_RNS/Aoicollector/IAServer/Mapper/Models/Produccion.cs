using CollectorPackage.Aoicollector.IAServer.Mapper.Models.WipModel;
using CollectorPackage.Src.Util.Convertion;
using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models
{
    public class Produccion
    {
        [JsonProperty("id_produccion")]
        public int id_produccion { get; set; }

        [JsonProperty("active")]
        public int? active { get; set; }

        [JsonProperty("barcode")]
        public string barcode { get; set; }

        [JsonProperty("linea")]
        public string linea { get; set; }

        [JsonProperty("numero_linea")]
        public int numero_linea { get; set; }

        [JsonProperty("op")]
        public string op { get; set; }

        [JsonProperty("line_id")]
        public int line_id { get; set; }

        [JsonProperty("puesto_id")]
        public int puesto_id { get; set; }

        [JsonProperty("modelo_id")]
        public int modelo_id { get; set; }

        [JsonProperty("id_stocker")]
        public string id_stocker { get; set; }

        [JsonProperty("inf")]
        public string inf { get; set; }

        [JsonProperty("manual_mode")]
        public string manual_mode { get; set; }

        [JsonProperty("tipo")]
        public string tipo { get; set; }

        [JsonProperty("id_maquina")]
        public int id_maquina { get; set; }

        [JsonProperty("puesto")]
        public string puesto { get; set; }

        [JsonProperty("declara")]
        public string declara { get; set; }

        [JsonProperty("id_user")]
        public int? id_user { get; set; }

        [JsonProperty("cogiscan")]
        public string cogiscan { get; set; }

        [JsonProperty("inspector")]
        public Inspector inspector { get; set; }

        [JsonProperty("smt")]
        public Smt smt { get; set; }

        [JsonProperty("wip")]
        public Wip wip { get; set; }

        [JsonProperty("sfcs")]
        public Sfcs sfcs { get; set; }

        [JsonProperty("route")]
        public Route route { get; set; }
    }
}
