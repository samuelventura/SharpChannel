using System;
using System.Threading;
using System.Collections.Generic;
using SharpChannel.Manager.Instance;
using SharpChannel.Tools;

namespace SharpChannel.Manager
{
    public class ChannelManager : IDisposable
    {
        private readonly Dictionary<int, Controller> controllers = new Dictionary<int, Controller>();
        private readonly Dictionary<int, string> setups = new Dictionary<int, string>();
        private readonly Action<int, string> states;
        private readonly ThreadRunner runner;
        private readonly Logger logger;

        public ChannelManager(Action<int, string> states = null)
        {
            this.states = states;
            var logfile = Executable.Relative("logs", "ChannelManager.txt");
            this.logger = new Logger(PatternLogFormatter.TIMESTAMP_LEVEL_LINE);
            this.logger.AddAppender(new ConsoleLogAppender());
            this.logger.AddAppender(new WriterLogAppender(logfile));
            this.runner = new ThreadRunner("Manager", OnError, OnIdle);
        }

        public Logger Logger { get { return logger;  } }

        public void Dispose()
        {
            runner.Dispose(() => {
                foreach (var controller in controllers.Values)
                {
                    Disposer.Dispose(controller);
                }
                controllers.Clear();
                setups.Clear();
            });
        }

        public void Update(ChannelInstance instance)
        {
            runner.Run(() => {
                var setup = ChannelUtils.SetupString(instance);
                setups.TryGetValue(instance.Id, out string current);
                if (current == null || current != setup)
                {
                    OnDelete(instance.Id);
                    string ip = ChannelUtils.IPAccess(instance.Access);
                    if (ip != null)
                    {
                        string executable = ChannelUtils.ExecutableName(instance.Type);
                        var arguments = string.Format("{0} {1} {2} {3}", ip, instance.Port, executable, instance.Config);
                        states?.Invoke(instance.Id, string.Empty);
                        setups[instance.Id] = setup;
                        controllers[instance.Id] = new Controller("SharpChannel.Manager.Instance", arguments);
                        logger.Info("{0} Endpoint {1}:{2}", instance.Id, ip, instance.Port);
                        logger.Info("{0} Executable {1}", instance.Id, executable);
                        logger.Info("{0} Config {1}", instance.Id, instance.Config);
                    }
                }
            });
        }

        public void Delete(int id)
        {
            runner.Run(() => OnDelete(id));
        }

        public void OnDelete(int id)
        {
            if (controllers.ContainsKey(id))
            {
                Disposer.Dispose(controllers[id]);
                controllers.Remove(id);
                states?.Invoke(id, null);
                setups.Remove(id);
                logger.Info("{0} Removed", id);
            }
        }

        private void OnIdle()
        {
            foreach (var id in controllers.Keys)
            {
                var controller = controllers[id];
                var state = controller.ReadLine();
                while (state != null)
                {
                    states?.Invoke(id, state);
                    logger.Info("{0} State {1}", id, state);
                    state = controller.ReadLine();
                }
                var error = controller.ReadError();
                while (error != null)
                {
                    logger.Warn("{0} {1}", id, error);
                    error = controller.ReadError();
                }
            }
            Thread.Sleep(10);
        }

        private void OnError(Exception ex)
        {
            logger.Error(ex.ToString());
        }
    }
}
