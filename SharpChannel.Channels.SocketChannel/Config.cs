using System;
using System.Net;
using System.Collections.Generic;

namespace SharpChannel.Channels.SocketChannel
{
    public class Config
    {
        public Config(string ip, int port)
        {
            IP = ip;
            Port = port;
        }

        public Config()
        {
            IP = "127.0.0.1";
            Port = 0;
        }

        public Config(Config sc)
        {
            CopyFrom(sc);
        }

        public string IP { get; set; }

        public int Port { get; set; }

        public void CopyFrom(Config sc)
        {
            IP = sc.IP;
            Port = sc.Port;
        }

        public override string ToString()
        {
            return string.Format("IP={0},Port={1}", IP, Port);
        }

        public static Config Parse(string text)
        {
            var parts = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var map = new Dictionary<string, string>();
            foreach (var part in parts)
            {
                var eq = part.IndexOf('=');
                map.Add(part.Substring(0, eq), part.Substring(eq + 1));
            }
            var config = new Config();
            Set(map, "IP", (value) => {
                config.IP = IPAddress.Parse(value).ToString();
            });
            Set(map, "Port", (value) => {
                config.Port = Convert.ToUInt16(value);
            });
            return config;
        }

        private static void Set(Dictionary<string, string> map, string prop, Action<string> action)
        {
            if (map.ContainsKey(prop))
            {
                action(map[prop]);
            }
        }

    }
}