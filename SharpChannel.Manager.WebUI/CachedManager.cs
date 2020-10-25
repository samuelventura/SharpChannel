using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SharpChannel.Tools;

namespace SharpChannel.Manager.WebUI
{
    public class CachedManager
    {
        private readonly object locker = new object();

        private readonly ChannelManager manager;
        private readonly ChannelPersistor persistor;
        private readonly Dictionary<int, ChannelInstance> instances;
        private readonly Dictionary<int, string> states;
        private readonly List<ChannelPlugin> plugins;

        public CachedManager()
        {
            manager = new ChannelManager(OnState);
            persistor = new ChannelPersistor();
            instances = new Dictionary<int, ChannelInstance>();
            states = new Dictionary<int, string>();
            foreach (var instance in persistor.List())
            {
                instances[instance.Id] = instance;
                manager.Update(instance);
            }
            plugins = FindPlugins();
        }

        public List<ChannelPlugin> Plugins { get { return plugins; } }

        public int Save(ChannelInstance instance)
        {
            lock (locker)
            {
                var id = persistor.Save(instance);
                instance = persistor.Load(id);
                instances[id] = instance;
                manager.Update(instance);
                return id;
            }
        }
        public void Update(int id, ChannelAccess access)
        {
            lock (locker)
            {
                instances.TryGetValue(id, out ChannelInstance instance);
                if (instance == null) return;
                instance.Access = access;
                persistor.Save(instance);
                instance = persistor.Load(id);
                instances[id] = instance;
                manager.Update(instance);
            }
        }

        public void Delete(int id)
        {
            lock (locker)
            {
                persistor.Delete(id);
                instances.Remove(id);
                manager.Delete(id);
            }
        }

        public ChannelModel LoadById(int id)
        {
            lock (locker)
            {
                instances.TryGetValue(id, out ChannelInstance instance);
                if (instance == null) return null;
                states.TryGetValue(instance.Id, out string state);
                return new ChannelModel(instance, state);
            }
        }

        public ChannelModel LoadByName(string name)
        {
            lock (locker)
            {
                foreach (var instance in instances.Values)
                {
                    if (instance.Name == name)
                    {
                        states.TryGetValue(instance.Id, out string state);
                        return new ChannelModel(instance, state);
                    }
                }
                return null;
            }
        }

        public List<ChannelModel> List()
        {
            lock (locker)
            {
                var list = new List<ChannelModel>();
                foreach (var instance in instances.Values)
                {
                    states.TryGetValue(instance.Id, out string state);
                    list.Add(new ChannelModel(instance, state ?? string.Empty));
                }
                return list;
            }
        }

        private void OnState(int id, string state)
        {
            lock (locker)
            {
                if (state == null) states.Remove(id);
                else states[id] = state;
            }
        }

        private List<ChannelPlugin> FindPlugins()
        {
            var list = new List<ChannelPlugin>();
            AddPlugins(list);
            AddThirdPartyPlugins(list);
            return list;
        }

        private void AddPlugins(List<ChannelPlugin> list)
        {
            var folder = Executable.Relative("plugins");
            var regex = new Regex(@"(.*)Channel\.txt");
            var glob = @"*Channel.txt";
            var files = Directory.GetFiles(folder, glob, SearchOption.TopDirectoryOnly);
            foreach (var path in files)
            {
                var file = Path.GetFileName(path);
                var match = regex.Match(file);
                if (!match.Success) continue;
                var type = match.Groups[1].Value;
                var plugin = new ChannelPlugin
                {
                    Type = type,
                    Name = File.ReadAllText(path),
                    View = $"Edit{type}Channel",
                };
                manager.Logger.Info("Plugin {0} {1}", file, plugin.Name);
                list.Add(plugin);
            }
        }

        private void AddThirdPartyPlugins(List<ChannelPlugin> list)
        {
            //nancy looks for default here first!
            var views = Executable.Relative("views", "thirdparty");
            Directory.CreateDirectory(views);
            var folder = Executable.Relative("thirdparty");
            Directory.CreateDirectory(folder);
            var regex = new Regex(@"SharpChannel\.Channels\.(.*)Channel");
            var glob = @"SharpChannel.Channels.*Channel";
            var files = Directory.EnumerateDirectories(folder, glob, SearchOption.TopDirectoryOnly);
            foreach (var path in files)
            {
                var file = Path.GetFileName(path);
                var match = regex.Match(file);
                if (!match.Success) continue;
                var type = match.Groups[1].Value;
                var exe = Path.Combine(file, $"{file}.exe");
                var name = Path.Combine(file, "plugins", $"{type}Channel.txt");
                var view = Path.Combine(file, "views", $"Edit{type}Channel.html");
                if (!File.Exists(Path.Combine(folder, exe))) continue;
                if (!File.Exists(Path.Combine(folder, name))) continue;
                if (!File.Exists(Path.Combine(folder, view))) continue;
                File.Copy(Path.Combine(folder, view), Path.Combine(views, $"Edit{type}Channel.html"), true);
                var plugin = new ChannelPlugin
                {
                    Third = true,
                    Type = type,
                    Name = File.ReadAllText(Path.Combine(folder, name)),
                    View = $"Edit{type}Channel",
                };
                manager.Logger.Info("Plugin {0} {1}", name, plugin.Name);
                list.Add(plugin);
            }
        }
    }
}
