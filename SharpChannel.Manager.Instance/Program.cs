using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using SharpChannel.Tools;

namespace SharpChannel.Manager.Instance
{
    class Program
    {
        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine(((Exception)e.ExceptionObject).Message);
            Console.Error.Flush();
            Environment.Exit(1);
        }

        private const TaskCreationOptions opts = TaskCreationOptions.LongRunning;

        private static object errorLock = new object();
        private static object stateLock = new object();

        public static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            var joint = string.Join(" ", args);
            var parts = joint.Split(new char[] { ' ' }, 4);
            var ip = parts[0];
            var port = Convert.ToInt32(parts[1]);
            var executable = parts[2];
            var arguments = parts.Length > 3 ? parts[3] : string.Empty;

            WriteError("{0}:{1}", Readable.Make(ip), port);
            WriteError(Readable.Make(executable));
            WriteError(Readable.Make(arguments));

            var listener = new TcpListener(IPAddress.Parse(ip), port);

            var pid = Process.GetCurrentProcess().Id;

            Task.Factory.StartNew(() => AcceptLoop(pid, listener, () => BuildStartInfo(executable, arguments)), opts);

            var line = Console.ReadLine();

            while (line != null)
            {
                line = Console.ReadLine();
            }

            Environment.Exit(0);
        }

        private static ProcessStartInfo BuildStartInfo(string executable, string arguments)
        {
            var pi = new ProcessStartInfo(Executable.Relative(executable))
            {
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true
            };
            return pi;
        }

        private static void WriteError(string format, params object[] args)
        {
            lock (errorLock)
            {
                Console.Error.WriteLine(format, args);
                Console.Error.Flush();
            }
        }

        private static void WriteState(string format, params object[] args)
        {
            lock (stateLock)
            {
                Console.WriteLine(format, args);
                Console.Out.Flush();
            }
        }

        private static void AcceptLoop(int pid, TcpListener listener, Func<ProcessStartInfo> pir)
        {
            var disposer = new Disposer();
            disposer.Add(() => Environment.Exit(1));

            using (disposer)
            {
                while (true)
                {
                    try
                    {
                        listener.Start();
                        break;
                    }
                    catch (Exception ex)
                    {
                        WriteState("Error listening on {0}", listener.LocalEndpoint);
                        WriteError(ex.Message);
                        Thread.Sleep(5000);
                    }
                }

                var endpoint = listener.LocalEndpoint as IPEndPoint;
                SetState(endpoint, pid);

                var currentClient = null as TcpClient;
                var currentTask = null as Task;

                while (true)
                {
                    var accepted = listener.AcceptTcpClient();
                    Disposer.Dispose(currentClient);
                    if (currentTask != null) currentTask.Wait();
                    currentClient = accepted;
                    currentTask = Task.Factory.StartNew(() => SafeClientLoop(pid, endpoint, accepted, pir()), opts);
                }
            }
        }

        private static void SafeClientLoop(int pid, IPEndPoint endpoint, TcpClient client, ProcessStartInfo pi)
        {
            try
            {
                //dynamic service location
                ClientLoop(pid, endpoint, client, pi);
            }
            catch (Exception ex)
            {
                WriteError(Readable.Make(pi.FileName));
                WriteError(Readable.Make(pi.Arguments));
                WriteError(ex.Message);
            }
        }

        private static void ClientLoop(int pid, IPEndPoint local, TcpClient client, ProcessStartInfo pi)
        {
            var first = new Disposer();

            using (var disposer = new Disposer(() => { SetState(local, pid); }))
            {
                disposer.Add(first);
                disposer.Add(client);

                var remote = client.Client.RemoteEndPoint as IPEndPoint;

                SetState(local, pid, remote);

                var proc = Process.Start(pi);

                disposer.Add(proc);
                disposer.Add(proc.Kill);

                SetState(local, pid, remote, proc.Id);

                var writeTask = Task.Factory.StartNew(() => WriteLine(proc, client), opts);
                var readTask = Task.Factory.StartNew(() => ReadLine(proc, client), opts);
                var errorTask = Task.Factory.StartNew(() => ReadError(proc, client), opts);

                first.Add(writeTask);
                first.Add(readTask);
                first.Add(errorTask);

                while (true)
                {
                    if (writeTask.IsCompleted) break;
                    if (readTask.IsCompleted && errorTask.IsCompleted) break;
                    Thread.Sleep(10);
                }
            }
        }

        private static void WriteLine(Process proc, TcpClient client)
        {
            var bytes = new byte[4096];

            while (true)
            {
                var count = client.GetStream().Read(bytes, 0, bytes.Length);
                if (count <= 0) return;
                var line = Convert.ToBase64String(bytes, 0, count);
                proc.StandardInput.WriteLine(line);
            }
        }

        private static void ReadLine(Process proc, TcpClient client)
        {
            var line = proc.StandardOutput.ReadLine();

            while (line != null)
            {
                var bytes = Convert.FromBase64String(line);
                client.GetStream().Write(bytes, 0, bytes.Length);
                line = proc.StandardOutput.ReadLine();
            }
        }

        private static void ReadError(Process proc, TcpClient client)
        {
            var line = proc.StandardError.ReadLine();

            while (line != null)
            {
                WriteError(line);
                line = proc.StandardError.ReadLine();
            }
        }

        private static void SetState(IPEndPoint local, int pid)
        {
            WriteState("{0}:{1}", local, pid);
        }

        private static void SetState(IPEndPoint local, int pid, IPEndPoint remote)
        {
            WriteState("{0}:{1} {2}", local, pid, remote);
        }

        private static void SetState(IPEndPoint local, int pid, IPEndPoint remote, int pid2)
        {
            WriteState("{0}:{1} {2}:{3}", local, pid, remote, pid2);
        }
    }
}