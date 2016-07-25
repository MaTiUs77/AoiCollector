using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AOI_VTWIN_RNS.Aoicollector.IAServer
{
    class ProductionService : IAServer
    {
        public IEnumerable<XElement> GetAoiProduction(string aoibarcode)
        {
            string path = url + "production/" + aoibarcode;
            result = Consume(path);

            return result;
        }

        public string Op()
        {
            return ReadTag("op", result.Elements("produccion"));
        }

        public bool Declara()
        {
            int declara = Convert.ToInt32(ReadTag("declara", result.Elements("sfcs")));
            return Convert.ToBoolean(declara);
        }

        public bool Active()
        {
            int active = Convert.ToInt32(ReadTag("active", result.Elements("wipot")));
            return Convert.ToBoolean( active );
        }

        public string SfcsPuestoId()
        {
            return ReadTag("puesto_id", result.Elements("produccion"));
        }

        public string SfcsLineId()
        {
            return ReadTag("line_id", result.Elements("produccion"));
        }
    }
}
