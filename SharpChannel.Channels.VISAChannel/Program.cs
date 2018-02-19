using System;
using System.Text;
using NationalInstruments.Visa;
using Ivi.Visa;

namespace SharpChannel.Channels.VISAChannel
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

            switch (cmdline)
            {
                case "--list":
                    using (var manager = new ResourceManager())
                    {
                        foreach (var instID in manager.Find("?*instr"))
                        {
                            Console.WriteLine(instID);
                        }
                    }
                    return;
            }

            var instr = cmdline.Replace("InstrID=", "");

            using (var manager = new ResourceManager())
            {
                using (var session = manager.Open(instr))
                {
                    var line = Console.ReadLine();
                    while (line != null)
                    {
                        var bytes = Convert.FromBase64String(line);
                        var req = ASCIIEncoding.ASCII.GetString(bytes);
                        var msg = (IMessageBasedSession)session;
                        msg.FormattedIO.WriteLine(req);
                        if (req.Contains("?"))
                        {
                            var res = msg.FormattedIO.ReadLine();
                            bytes = ASCIIEncoding.ASCII.GetBytes(res);
                            line = Convert.ToBase64String(bytes, 0, bytes.Length);
                            Console.WriteLine(line);
                        }
                        line = Console.ReadLine();
                    }
                }
            }
        }
    }
}
