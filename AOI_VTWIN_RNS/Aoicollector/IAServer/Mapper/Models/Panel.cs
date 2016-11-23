using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorPackage.Aoicollector.IAServer.Mapper.Models
{
    public class Panel
    {
        [JsonProperty("id_panel_history")]
        public int id_panel_history { get; set; }

        [JsonProperty("modo")]
        public string modo { get; set; }

        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("id_maquina")]
        public int id_maquina { get; set; }

        [JsonProperty("panel_barcode")]
        public string panel_barcode { get; set; }

        [JsonProperty("programa")]
        public string programa { get; set; }

        [JsonProperty("fecha")]
        public string fecha { get; set; }

        [JsonProperty("hora")]
        public string hora { get; set; }

        [JsonProperty("turno")]
        public string turno { get; set; }

        [JsonProperty("revision_aoi")]
        public string revision_aoi { get; set; }

        [JsonProperty("revision_ins")]
        public string revision_ins { get; set; }

        [JsonProperty("errores")]
        public int errores { get; set; }

        [JsonProperty("falsos")]
        public int falsos { get; set; }

        [JsonProperty("reales")]
        public int reales { get; set; }

        [JsonProperty("bloques")]
        public int bloques { get; set; }

        [JsonProperty("etiqueta")]
        public string etiqueta { get; set; }

        [JsonProperty("pendiente_inspeccion")]
        public bool pendiente_inspeccion { get; set; }

        [JsonProperty("test_machine_id")]
        public int test_machine_id { get; set; }

        [JsonProperty("program_name_id")]
        public int program_name_id { get; set; }

        [JsonProperty("inspected_op")]
        public string inspected_op { get; set; }

        [JsonProperty("semielaborado")]
        public string semielaborado { get; set; }

        [JsonProperty("id_user")]
        public int? id_user { get; set; }

        [JsonProperty("created_date")]
        public string created_date { get; set; }

        [JsonProperty("created_time")]
        public string created_time { get; set; }

        [JsonProperty("linea")]
        public int linea { get; set; }
    }
}
