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
        private const TaskCreationOptions opts = TaskCreationOptions.LongRunning;

        private static object errorLock = new object();
        private static object outLock = new object();

        public static void Main(string[] args)
        {
            var joint = string.Join(" ", args);
            var parts = joint.Split(new char[] { ' ' }, 4);
            var ip = parts[0];
            var port = Convert.ToInt32(parts[1]);
            var executable = parts[2];
            var arguments = parts.Length > 3 ? parts[3] : string.Empty;

            WriteError("Endpoint <{0}>:<{1}>", ip, port);
            WriteError("Executable <{0}>", executable);
            WriteError("Arguments <{0}>", arguments);

            var listener = new TcpListener(IPAddress.Parse(ip), port);

            var pid = Process.GetCurrentProcess().Id;

            Task.Factory.StartNew(() => AcceptLoop(pid, listener, () => BuildStartInfo(executable, arguments)), opts);

            var line = Console.ReadLine();

            while (line != null)
            {
                line = Console.ReadLine();
            }
        }

        private static ProcessStartInfo BuildStartInfo(string executable, string arguments)
        {
            var pi = new ProcessStartInfo(Executable.Relative(executable))
            {
                Arguments = string.Format("{0}", arguments),
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

        private static void WriteLine(string format, params object[] args)
        {
            lock (outLock)
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
                        WriteLine("Error listening on {0}", listener.LocalEndpoint);
                        WriteError(ex.Message);
                        Thread.Sleep(5000);
                    }
                }
                var endpoint = listener.LocalEndpoint as IPEndPoint;
                State(endpoint, pid);

                TcpClient currentClient = null;
                Task currentTask = null;

                while (true)
                {
                    var accepted = listener.AcceptTcpClient();
                    Disposer.Dispose(currentClient);
                    if (currentTask != null) currentTask.Wait();
                    currentClient = accepted;
                    currentTask = Task.Factory.StartNew(() => SafeClientLoop(pid, endpoint, accepted, pir()), opts)
                        .ContinueWith((p) => State(endpoint, pid));
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
                WriteError("<{0}> <{1}>", pi.FileName, pi.Arguments);
                WriteError(ex.Message);
            }
        }

        private static void ClientLoop(int pid, IPEndPoint local, TcpClient client, ProcessStartInfo pi)
        {
            var disposer = new Disposer();
            disposer.Add(client);

            using (disposer)
            {
                var remote = client.Client.RemoteEndPoint as IPEndPoint;

                State(local, pid, remote);

                var proc = Process.Start(pi);

                disposer.Add(proc);
                disposer.Add(proc.Kill);

                State(local, pid, remote, proc.Id);

                var writeTask = Task.Factory.StartNew(() => WriteLine(proc, client), opts);
                disposer.Add(writeTask);
                var readTask = Task.Factory.StartNew(() => ReadLine(proc, client), opts);
                disposer.Add(readTask);
                var errorTask = Task.Factory.StartNew(() => ReadError(proc, client), opts);
                disposer.Add(errorTask);

                while (client.Connected && !proc.HasExited) Thread.Sleep(10);
            }
        }

        private static void WriteLine(Process proc, TcpClient client)
        {
            var disposer = new Disposer();
            disposer.Add(proc);
            disposer.Add(client);
            disposer.Add(proc.Kill);

            using (disposer)
            {
                var bytes = new byte[4096];
                while (!proc.HasExited)
                {
                    var count = client.GetStream().Read(bytes, 0, bytes.Length);
                    if (count <= 0) return;
                    var line = Convert.ToBase64String(bytes, 0, count);
                    proc.StandardInput.WriteLine(line);
                }
            }
        }

        private static void ReadLine(Process proc, TcpClient client)
        {
            var disposer = new Disposer();
            disposer.Add(proc);
            disposer.Add(client);
            disposer.Add(proc.Kill);

            using (disposer)
            {
                var line = proc.StandardOutput.ReadLine();

                while (line != null)
                {
                    var bytes = Convert.FromBase64String(line);
                    client.GetStream().Write(bytes, 0, bytes.Length);
                    line = proc.StandardOutput.ReadLine();
                }
            }
        }

        private static void ReadError(Process proc, TcpClient client)
        {
            var disposer = new Disposer();
            disposer.Add(proc);
            disposer.Add(client);
            disposer.Add(proc.Kill);

            using (disposer)
            {
                var line = proc.StandardError.ReadLine();

                while (line != null)
                {
                    WriteError(line);
                    line = proc.StandardError.ReadLine();
                }
            }
        }

        private static void State(IPEndPoint local, int pid)
        {
            WriteLine("{0}:{1}", local, pid);
        }

        private static void State(IPEndPoint local, int pid, IPEndPoint remote)
        {
            WriteLine("{0}:{1} {2}", local, pid, remote);
        }

        private static void State(IPEndPoint local, int pid, IPEndPoint remote, int pid2)
        {
            WriteLine("{0}:{1} {2}:{3}", local, pid, remote, pid2);
        }
    }
}