using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Net;

namespace CollectorPackage.Src.Service
{
    public class ServiceJson
    {
        public bool hasResponse = false;

        public string Consume(string route)
        {
            string jsonData = WebDownload(route);
            return jsonData;
        }

        public string WebDownload(string url)
        {
            byte[] data;
            using (WebClient webClient = new WebClient())
                data = webClient.DownloadData(url);

            string str = Encoding.GetEncoding("Windows-1252").GetString(data);
            return str;
        }
    }
}
