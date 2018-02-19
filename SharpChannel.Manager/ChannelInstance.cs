using System;

namespace SharpChannel.Manager
{
    public class ChannelInstance
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ChannelAccess Access { get; set; }
        public int Port { get; set; }
        public string Type { get; set; }
        public string Config { get; set; }

        public ChannelInstance()
        {

        }

        public ChannelInstance(string type)
        {
            Type = type;
        }
    }
}
