using System.Collections.Generic;
using SharpChannel.Manager;
using SharpChannel.Tools;
using LiteDB;

namespace SharpChannel.Manager.WebUI
{
    public class ChannelPersistor
    {
        private const string CHANNELS = "channels";

        public int Save(ChannelInstance instance)
        {
            using (var db = new LiteDatabase(DbPath()))
            {
                var Channels = db.GetCollection<ChannelInstance>(CHANNELS);
                if (instance.Id == 0) return Channels.Insert(instance);
                else Channels.Upsert(instance);
                return instance.Id;
            }
        }

        public bool Delete(int id)
        {
            using (var db = new LiteDatabase(DbPath()))
            {
                var Channels = db.GetCollection<ChannelInstance>(CHANNELS);
                return Channels.Delete(id);
            }
        }

        public ChannelInstance Load(int id)
        {
            using (var db = new LiteDatabase(DbPath()))
            {
                var Channels = db.GetCollection<ChannelInstance>(CHANNELS);
                return Channels.FindById(id);
            }
        }

        public List<ChannelInstance> List()
        {
            using (var db = new LiteDatabase(DbPath()))
            {
                var Channels = db.GetCollection<ChannelInstance>(CHANNELS);
                return new List<ChannelInstance>(Channels.FindAll());
            }
        }

        private string DbPath()
        {
            return Executable.Relative("SharpChannel.Manager.db");
        }
    }
}
