using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
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
                    Console.Out.Flush();
                    Environment.Exit(0);
                    return;
            }

            var instrID = cmdline.Replace("InstrID=", "");
            
            using (var manager = new ResourceManager())
            {
                using (var session = manager.Open(instrID))
                {
                    var msg = session as IMessageBasedSession;
                    msg.LockResource();
                    msg.Clear();

                    var thread = new Thread(() => { CheckLoop(instrID); })
                    {
                        IsBackground = true
                    };
                    thread.Start();

                    var list = new List<byte>();
                    var line = Console.ReadLine();
                    while (line != null)
                    {
                        var bytes = Convert.FromBase64String(line);
                        foreach(var b in bytes)
                        {
                            list.Add(b);
                            if(b == '\n')
                            {
                                var req = Encoding.ASCII.GetString(list.ToArray());
                                req = req.Trim();
                                msg.FormattedIO.WriteLine(req);
                                if (req.Contains("?"))
                                {
                                    var res = msg.FormattedIO.ReadLine();
                                    var bytes2 = Encoding.ASCII.GetBytes(res);
                                    var line2 = Convert.ToBase64String(bytes2, 0, bytes2.Length);
                                    Console.WriteLine(line2);
                                }
                                list.Clear();
                            }
                        }
                        line = Console.ReadLine();
                    }
                }
            }

            throw new Exception("Stdin closed unexpectedly");
        }

        private static void CheckLoop(string instrID)
        {
            while (true)
            {
                var found = false;
                using (var manager = new ResourceManager())
                {
                    foreach (var id in manager.Find("?*instr"))
                    {
                        if (id == instrID) found = true;
                    }
                }
                if (!found) throw new Exception("Instrument removed");
                Thread.Sleep(200);
            }
        }
    }
}
