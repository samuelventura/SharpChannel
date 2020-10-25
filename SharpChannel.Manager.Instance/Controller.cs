using System;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using SharpChannel.Tools;

namespace SharpChannel.Manager.Instance
{
    public class Controller : IDisposable
    {
        private const TaskCreationOptions opts = TaskCreationOptions.LongRunning;

        private readonly ConcurrentQueue<string> state = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> error = new ConcurrentQueue<string>();

        private readonly Task task;

        private volatile bool disposed;

        public Controller(string executable, string arguments)
        {
            var filepath = Executable.Relative(executable);
            var pi = new ProcessStartInfo(filepath)
            {
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WorkingDirectory = Path.GetDirectoryName(filepath),
            };
            task = Task.Factory.StartNew(() => Loop(pi), opts);
        }

        public void Dispose()
        {
            disposed = true;
            task.Wait();
        }

        public string ReadState()
        {
            state.TryDequeue(out string line);
            return line;
        }

        public string ReadError()
        {
            error.TryDequeue(out string line);
            return line;
        }

        private void Loop(ProcessStartInfo pi)
        {
            while (!disposed)
            {
                try
                {
                    var first = new Disposer();

                    using (var disposer = new Disposer())
                    {
                        disposer.Add(first);

                        var proc = Process.Start(pi);

                        disposer.Add(proc);
                        disposer.Add(proc.Kill);

                        var stateTask = Task.Factory.StartNew(() => ReadState(proc), opts);
                        var errorTask = Task.Factory.StartNew(() => ReadError(proc), opts);

                        first.Add(stateTask);
                        first.Add(errorTask);

                        while (true)
                        {
                            if (disposed) break;
                            if (stateTask.IsCompleted && errorTask.IsCompleted) break;
                            Thread.Sleep(10);
                        }
                    }
                }
                catch (Exception ex)
                {
                    state.Enqueue(ex.Message);
                    error.Enqueue(Readable.Make(pi.FileName));
                    error.Enqueue(Readable.Make(pi.Arguments));
                    error.Enqueue(ex.Message);
                    var millis = 5000;
                    while (--millis > 0)
                    {
                        if (disposed) break;
                        Thread.Sleep(1);
                    }
                }
            }
        }

        private void ReadState(Process proc)
        {
            var line = proc.StandardOutput.ReadLine();

            while (line != null)
            {
                state.Enqueue(line);

                line = proc.StandardOutput.ReadLine();
            }
        }

        private void ReadError(Process proc)
        {
            var line = proc.StandardError.ReadLine();

            while (line != null)
            {
                error.Enqueue(line);

                line = proc.StandardError.ReadLine();
            }
        }
    }
}
