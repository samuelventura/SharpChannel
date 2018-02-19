using System;

namespace SharpChannel.Channels.EchoChannel
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

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "--list":
                        return;
                }
            }

            var line = Console.ReadLine();

            while (line != null)
            {
                WriteLine(line);
                line = Console.ReadLine();
            }

            throw new Exception("Stdin closed unexpectedly");
        }

        private static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.Out.Flush();
        }
    }
}
