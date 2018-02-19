using System;
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

        private readonly ConcurrentQueue<string> input = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> error = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> output = new ConcurrentQueue<string>();

        private readonly Task task;

        private volatile bool disposed;

        public Controller(string executable, string arguments)
        {
            var filename = Executable.Relative(executable);
            var pi = new ProcessStartInfo(filename)
            {
                Arguments = string.Format("{0}", arguments),
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };
            task = Task.Factory.StartNew(() => Loop(pi), opts);
        }

        public void Dispose()
        {
            disposed = true;
            task.Wait();
        }

        public string ReadLine()
        {
            input.TryDequeue(out string line);
            return line;
        }

        public string ReadError()
        {
            error.TryDequeue(out string line);
            return line;
        }

        public void WriteLine(string format, params object[] args)
        {
            var line = args.Length > 0 ? string.Format(format, args) : format;
            output.Enqueue(line);
        }

        private void Loop(ProcessStartInfo pi)
        {
            while (!disposed)
            {
                try
                {
                    var disposer = new Disposer();

                    using (disposer)
                    {
                        var proc = Process.Start(pi);

                        var writeTask = Task.Factory.StartNew(() => WriteLine(proc), opts);
                        var readTask = Task.Factory.StartNew(() => ReadLine(proc), opts);
                        var errorTask = Task.Factory.StartNew(() => ReadError(proc), opts);

                        disposer.Add(writeTask);
                        disposer.Add(readTask);
                        disposer.Add(errorTask);

                        //dispose flag should start by killing proc
                        disposer.Add(proc);
                        disposer.Add(proc.Kill);

                        while (!disposed && !proc.HasExited) Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    input.Enqueue(ex.Message);
                    error.Enqueue(string.Format("<{0}> <{1}>", pi.FileName, pi.Arguments));
                    error.Enqueue(ex.Message);
                    var millis = 5000;
                    while (!disposed && --millis > 0) Thread.Sleep(1);
                }
            }
        }

        private void WriteLine(Process proc)
        {
            var disposer = new Disposer();
            disposer.Add(proc);
            disposer.Add(proc.Kill);

            using (disposer)
            {
                while (!proc.HasExited)
                {
                    output.TryDequeue(out string line);

                    if (line == null) { Thread.Sleep(10); continue; }

                    proc.StandardInput.WriteLine(line);
                }
            }
        }

        private void ReadLine(Process proc)
        {
            var disposer = new Disposer();
            disposer.Add(proc);
            disposer.Add(proc.Kill);

            using (disposer)
            {
                var line = proc.StandardOutput.ReadLine();

                while (line != null)
                {
                    input.Enqueue(line);

                    line = proc.StandardOutput.ReadLine();
                }
            }
        }

        private void ReadError(Process proc)
        {
            var disposer = new Disposer();
            disposer.Add(proc);
            disposer.Add(proc.Kill);

            using (disposer)
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
}
