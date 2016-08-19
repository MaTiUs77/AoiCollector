using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Configuration;
using System.Collections.Specialized;
using System.Net;
using System.Xml.Linq;

namespace AOI_VTWIN_RNS.Src.Http
{
    class Http
    {
        public static string LoadXMLFromUrl(string url)
        {
            byte[] data;
            using (WebClient webClient = new WebClient())
                data = webClient.DownloadData(url);

            string str = Encoding.GetEncoding("Windows-1252").GetString(data);
            return str;
        }
    }
}
