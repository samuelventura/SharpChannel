using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SharpChannel.Manager.Service
{
    [RunInstaller(true)]
    public class Install : Installer
    {
        public Install()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.DisplayName = "Channel Manager Service";
            serviceInstaller.ServiceName = Program.NAME;

            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
