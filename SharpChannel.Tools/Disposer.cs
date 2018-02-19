using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SharpChannel.Tools
{
    public class Disposer : IDisposable
    {
        // SerialPort, Socket, TcpClient, Streams, Writers, Readers, ...
        public static void Dispose(object disposable)
        {
            Execute(() => {
                if (disposable is IDisposable)
                    ((IDisposable)disposable).Dispose();
            });
        }

        // TcpListener
        public static void Stop(TcpListener stoppable)
        {
            Execute(() => {
                if (stoppable != null)
                    stoppable.Stop();
            });
        }

        public static void Execute(Action action)
        {
            try { action(); } catch (Exception) { }
        }

        private readonly List<Action> actions;

        public Disposer(params Action[] actions)
        {
            this.actions = new List<Action>(actions);
        }

        public void Add(IDisposable disposable)
        {
            actions.Add(() => {
                disposable?.Dispose();
            });
        }

        public void Add(Action action)
        {
            actions.Add(action);
        }

        public void Dispose()
        {
            actions.Reverse();
            foreach (var action in actions) Execute(action);
            actions.Clear();
        }
    }
}
