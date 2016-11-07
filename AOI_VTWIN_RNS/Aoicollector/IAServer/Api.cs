using AOI_VTWIN_RNS.Src.Config;
using AOI_VTWIN_RNS.Src.Service;
using System;

namespace AOI_VTWIN_RNS.Aoicollector
{
    public class Api : ServiceJson
    {
        public static string apiUrl = "";

        public Api()
        {
            if(apiUrl.Equals(""))
            {
                apiUrl = AppConfig.Read("SERVICE", "url");
            }
        }

        public Exception error { get; set; }
        public new bool hasResponse { get; set; }        
    }
}
