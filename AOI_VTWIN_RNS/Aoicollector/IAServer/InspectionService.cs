using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AOI_VTWIN_RNS.Aoicollector.IAServer
{
    class InspectionService : IAServer
    {
        /// <summary>
        /// Datos de inspeccion de panel
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns>IEnumerable<XElement></returns>
        public IEnumerable<XElement> GetInspectionInfo(string barcode)
        {
            string path = url + barcode;
            result = Consume(path);
            return result;
        }

        public int IdPanel()
        {
            IEnumerable<XElement> aoi = result.Elements("aoi");
            string id_panel = ReadTag("id", aoi.Elements("panel"));
            return Convert.ToInt32(id_panel);
        }

        public string AssignedOp()
        {
            IEnumerable<XElement> aoi = result.Elements("aoi");
            return ReadTag("inspected_op", aoi.Elements("panel"));
        }        
    }
}
