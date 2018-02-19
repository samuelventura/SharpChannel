using System;

namespace SharpChannel.Manager.WebUI
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var launcher = new NancyLauncher())
            {
                launcher.Start();
                Console.WriteLine("Running on {0}", launcher.URI);
                Console.ReadLine();
            }
        }
    }
}
