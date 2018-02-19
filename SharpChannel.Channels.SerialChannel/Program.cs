using System;
using System.IO.Ports;
using System.Threading;
using SharpChannel.Tools;

namespace SharpChannel.Channels.SerialChannel
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
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            var cmdline = string.Join(" ", args);

            switch(cmdline)
            {
                case "--list":
                    var names = SerialPort.GetPortNames();
                    foreach (var name in names) Console.WriteLine(name);
                    Console.Out.Flush();
                    return;
            }

            var serial = new SerialPort();
            Config.Parse(serial, cmdline);
            serial.Open();

            var thread = new Thread(()=> { ReadLoop(serial);  });
            thread.IsBackground = true;
            thread.Start();

            var line = Console.ReadLine();

            while (line != null)
            {
                var bytes = Convert.FromBase64String(line);
                serial.Write(bytes, 0, bytes.Length);
                line = Console.ReadLine();
            }

            throw new Exception("Stdin closed unexpectedly");
        }

        private static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.Out.Flush();
        }

        private static void ReadLoop(SerialPort serial)
        {
            var bytes = new byte[4096];

            while (true)
            {
                var count = serial.Read(bytes, 0, bytes.Length);
                if (count <= 0) throw new Exception("Serial port closed unexpectedly");
                var line = Convert.ToBase64String(bytes, 0, count);
                WriteLine(line);
            }
        }
    }
}