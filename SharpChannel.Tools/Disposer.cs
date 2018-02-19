using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace SharpChannel.Tools
{
    public class Disposer : IDisposable
    {
        // SerialPort, Socket, TcpClient, Streams, Writers, Readers, ...
        public static void Dispose(IDisposable disposable)
        {
            Execute(() => {
                if (disposable != null)
                    disposable.Dispose();
            });
        }

        // TcpListener
        public static void Close(TcpListener closeable)
        {
            Execute(() => {
                if (closeable != null)
                    closeable.Stop();
            });
        }

        private static void Execute(Action action)
        {
            try { action(); } catch (Exception) { }
        }

        private readonly List<Action> actions;

        public Disposer()
        {
            this.actions = new List<Action>();
        }

        public void Add(IDisposable disposable)
        {
            actions.Add(() => {
                disposable.Dispose();
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
