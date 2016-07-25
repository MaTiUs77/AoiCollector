using System;
using System.Collections.Generic;
using System.Xml.Linq;

using AOI_VTWIN_RNS.Src.Config;
using AOI_VTWIN_RNS.Src.Service;

namespace AOI_VTWIN_RNS.Aoicollector.IAServer
{
    class IAServer : Service
    {
        public string url { get; set; }
        public IEnumerable<XElement> result = null;

        public bool hasResponse = false;

        public IAServer() 
        {
            url = AppConfig.Read("SERVICE", "url");
        }
    }
}
