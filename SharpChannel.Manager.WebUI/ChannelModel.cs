
namespace SharpChannel.Manager.WebUI
{
    public class ChannelModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Access { get; set; }
        public int Port { get; set; }
        public string Type { get; set; }
        public string Config { get; set; }
        public string State { get; set; }

        public ChannelModel(ChannelInstance instance, string state = null)
        {
            this.Id = instance.Id;
            this.Name = instance.Name;
            this.Access = instance.Access.ToString();
            this.Port = instance.Port;
            this.Type = instance.Type;
            this.Config = instance.Config;
            this.State = state;
        }
    }
}