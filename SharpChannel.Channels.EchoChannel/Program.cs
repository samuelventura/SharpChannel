using System;

namespace SharpChannel.Channels.EchoChannel
{
    class Program
    {
        public static void Main(string[] args)
        {
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
        }

        private static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            Console.Out.Flush();
        }
    }
}
