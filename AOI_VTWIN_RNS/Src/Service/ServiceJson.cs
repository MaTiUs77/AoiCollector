using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml.Serialization;

namespace AOI_VTWIN_RNS.Src.Service
{
    public class ServiceJson
    {
        public bool hasResponse = false;

        public static string Consume(string route)
        {
            string jsonData = Http.Http.LoadXMLFromUrl(route + "?json");
            return jsonData;
        }
    }
}
