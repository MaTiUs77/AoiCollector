using System.Text;
using System.Net;

namespace CollectorPackage.Src.Http
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
