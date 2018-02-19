using System;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Reflection;
using System.IO;
using SharpChannel.Manager.WebUI;
using SharpChannel.Tools;

namespace SharpChannel.Manager.Service
{
    class Program : ServiceBase
    {
        public static readonly string NAME = "ChannelManager";

        private NancyLauncher launcher;

        public Program()
        {
            this.ServiceName = NAME;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            launcher = new NancyLauncher();
            launcher.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();

            launcher.Dispose();
        }

        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;


            if (Environment.UserInteractive)
            {
                string parameter = string.Concat(args);

                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                }
            }
            else
            {
                ServiceBase.Run(new Program());
            }
        }
        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var filename = Executable.Relative("error.txt");
            File.AppendAllText(filename, ((Exception)e.ExceptionObject).ToString());
            File.AppendAllText(filename, ((Exception)e.ExceptionObject).InnerException.ToString());
        }
    }
}
