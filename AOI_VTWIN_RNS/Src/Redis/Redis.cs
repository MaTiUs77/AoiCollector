using StackExchange.Redis;
using System;

namespace AOICollector.Src.Redis
{
    public class Redis
    {
        private ConnectionMultiplexer con;
        private IDatabase db;
        private ISubscriber sub;

        public string host { get; set; }

        public Redis(string _host) {
            host = _host;
        }

        public void Connect()
        {
            try
            {
                if (con == null || !con.IsConnected)
                {
                    con = ConnectionMultiplexer.Connect(host);
                    db = con.GetDatabase();
                    sub = con.GetSubscriber();

                    Console.WriteLine("Conectado");
                } else
                {
                    //Console.WriteLine("Ya se encuentra conectado");
                }       
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (con.IsConnected)
                {
                    con.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Subscribe(string channel)
        {
            try
            {
                sub.Subscribe(channel, (channelRespone, message) => {
                    Console.WriteLine(message);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Publish(string channel, string json)
        {
            if (sub != null)
            {
                try
                {
                    if (con.IsConnected)
                    {
                        sub.Publish(channel, json);
                    }
                    else
                    {
                        Console.WriteLine("No se encuentra conectado al servidor");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void Emit(string channel, string json)
        {
            //string json = JsonConvert.SerializeObject(this);

            if (db != null)
            {
                try
                {
                    if (con.IsConnected)
                    {
                        //db.StringSet("servermonitor:status:" + nombre, json);
                        Publish(channel, json);
                    }
                    else
                    {
                        Console.WriteLine("No se encuentra conectado al servidor");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
