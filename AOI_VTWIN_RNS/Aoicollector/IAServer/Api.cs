using CollectorPackage.Src.Config;
using CollectorPackage.Src.Service;
using System;

namespace CollectorPackage.Aoicollector
{
    public class Api : ServiceJson
    {
        public static string apiUrl = "";

        public Api()
        {
            if(apiUrl.Equals(""))
            {
                apiUrl = AppConfig.Read("IASERVER", "apiurl");
            }
        }

        public Exception error { get; set; }
        public new bool hasResponse { get; set; }        
    }
}
