using System;
using System.IO.Ports;
using System.Collections.Generic;

namespace SharpChannel.Channels.ModbusChannel
{
    public class Config
    {
        public static void Parse(SerialPort config, string text)
        {
            var parts = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var map = new Dictionary<string, string>();
            foreach (var part in parts)
            {
                var equals = part.Split('=');
                map.Add(equals[0], equals[1]);
            }
            Set(map, "PortName", (value) => {
                config.PortName = value;
            });
            Set(map, "BaudRate", (value) => {
                config.BaudRate = Convert.ToInt32(value);
            });
            Set(map, "DataBits", (value) => {
                config.DataBits = Convert.ToInt32(value);
            });
            Set(map, "Parity", (value) => {
                config.Parity = (Parity)Enum.Parse(typeof(Parity), value);
            });
            Set(map, "Handshake", (value) => {
                config.Handshake = (Handshake)Enum.Parse(typeof(Handshake), value);
            });
            Set(map, "StopBits", (value) => {
                config.StopBits = (StopBits)Enum.Parse(typeof(StopBits), value);
            });
            Set(map, "DtrEnable", (value) => {
                config.DtrEnable = Convert.ToBoolean(value);
            });
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