using AOICollector.Src.Redis;
using CollectorPackage.Src.Config;

namespace CollectorPackage.Aoicollector.Core
{
    public class Realtime
    {
        public static Redis redis = new Redis(AppConfig.Read("REDIS", "host"));

        public static string channel = "aoicollector:monitor";

        public static void send(string json)
        {
            redis.Connect();
            redis.Emit(channel, json.ToString());
        }
    }
}

