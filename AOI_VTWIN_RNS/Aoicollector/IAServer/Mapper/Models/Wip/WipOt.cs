using Newtonsoft.Json;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models.WipModel
{
    public class WipOt
    {
        [JsonProperty("ultimo_serie")]
        public string ultimo_serie { get; set; }

        [JsonProperty("ultimo_history")]
        public string ultimo_history { get; set; }

        [JsonProperty("organization_code")]
        public string organization_code { get; set; }

        [JsonProperty("nro_op")]
        public string nro_op { get; set; }

        [JsonProperty("start_quantity")]
        public int start_quantity { get; set; }

        [JsonProperty("quantity_completed")]
        public int quantity_completed { get; set; }

        [JsonProperty("alternate_bom_designator")]
        public string alternate_bom_designator { get; set; }

        [JsonProperty("primary_item_id")]
        public int primary_item_id { get; set; }

        [JsonProperty("codigo_producto")]
        public string codigo_producto { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("porcentaje")]
        public float porcentaje { get; set; }

        [JsonProperty("restante")]
        public int restante { get; set; }
    }
}
