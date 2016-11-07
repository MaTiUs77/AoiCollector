using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOI_VTWIN_RNS.Aoicollector.Rns.Controller
{
    class InspectionTextFileHeader
    {
        public string header = "";
        public List<InspectionTextFileAtrribute> attributes = new List<InspectionTextFileAtrribute>();

        public string FindAtrributeValue(string key)
        {
            return attributes.Where(o => o.variable == key).FirstOrDefault().valor;
        }
    }
}
