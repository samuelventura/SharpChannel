using System;
using System.Threading;
using System.Net.Sockets;
using SharpChannel.Tools;

namespace SharpChannel.Channels.SocketChannel
{
    class Program
    {
        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine(((Exception)e.ExceptionObject).Message);
            Console.Error.Flush();
            Environment.Exit(1);
        }

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "--list":
                        return;
                }
            }

            var config = Config.Parse(string.Join(" ", args));

            var socket = new TcpClient();
            var result = socket.BeginConnect(config.IP, config.Port, null, null);
            if (!result.AsyncWaitHandle.WaitOne(1000, true))
                Thrower.Throw("Timeout connecting to {0}:{1}", config.IP, config.Port);
            socket.EndConnect(result);

            var thread = new Thread(() => { ReadLoop(socket); })
            {
                IsBackground = true
            };
            thread.Start();

            var line = Console.ReadLine();

            while (line != null)
            {
                var bytes = Convert.FromBase64String(line);
                socket.GetStream().Write(bytes, 0, bytes.Length);
                line = Console.ReadLine();
            }

            throw new Exception("Stdin closed unexpectedly");
        }

        private static void ReadLoop(TcpClient socket)
        {
            var bytes = new byte[4096];

            while (true)
            {
                var count = socket.GetStream().Read(bytes, 0, bytes.Length);
                if (count <= 0) throw new Exception("Socket closed unexpectedly");
                var line = Convert.ToBase64String(bytes, 0, count);
                Console.WriteLine(line);
                Console.Out.Flush();
            }
        }
    }
}