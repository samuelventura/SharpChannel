using System;
using System.Threading;
using System.Collections.Generic;

namespace SharpChannel.Tools
{
    public interface IRunner
    {
        void Run(Action action);
    }

    public class ThreadRunner : IRunner, IDisposable
    {
        private readonly Queue<Named> msgs;
        private readonly Thread thread;
        private readonly Action idle;

        public ThreadRunner(string name)
            : this(name, null)
        {
        }

        public ThreadRunner(string name, Action<Exception> catcher, Action idle = null)
            : this(name, idle)
        {
        }

        public ThreadRunner(string name, Action idle = null)
        {
            this.idle = idle;
            this.msgs = new Queue<Named>();
            this.thread = new Thread(Loop)
            {
                IsBackground = true,
                Name = name
            };
            this.thread.Start();
        }

        public void Run(Action action)
        {
            Push(new Named("action", action));
        }

        public void Dispose(Action action)
        {
            Push(new Named("dispose", action));
            thread.Join();
        }

        public void Dispose()
        {
            Push(new Named("dispose"));
            thread.Join();
        }

        public void Flush()
        {
            var flag = new AutoResetEvent(false);
            Push(new Named("flush", flag));
            flag.WaitOne();
        }

        private void Loop()
        {
            var last = DateTime.Now;

            while (true)
            {
                var msg = Poll();
                if (msg == null)
                {
                    if (idle != null) Execute(idle);
                    else Thread.Sleep(1);
                }
                else
                {
                    switch (msg.Name)
                    {
                        case "action":
                            Execute((Action)msg.Payload);
                            break;
                        case "flush":
                            var flag = (AutoResetEvent)msg.Payload;
                            flag.Set();
                            break;
                        case "dispose":
                            var action = (Action)msg.Payload;
                            if (action != null) Execute(action);
                            return;
                    }
                }
            }
        }

        private void Push(Named msg)
        {
            lock (msgs)
            {
                msgs.Enqueue(msg);
            }
        }

        private Named Poll()
        {
            lock (msgs)
            {
                if (msgs.Count > 0) return msgs.Dequeue();
            }
            return null;
        }

        private void Execute(Action action)
        {
            try { action(); } catch (Exception) { }
        }
    }
}
