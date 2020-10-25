using System;
using Nancy.Hosting.Self;
using System.Configuration;
using SharpChannel.Tools;

namespace SharpChannel.Manager.WebUI
{
    public class NancyLauncher : IDisposable
    {
        private readonly NancyHost host;
        private readonly string uri;

        public string URI { get { return uri; } }

        public NancyLauncher()
        {
            //netsh http add urlacl url = http://+:8888/ user=Everyone
            var port = ConfigurationManager.AppSettings.Get("port");
            uri = string.Format("http://localhost:{0}", port);
            if (Profile.IsDebug()) uri = "http://localhost:8888";
            host = new NancyHost(new Uri(uri));
        }

        public void Start()
        {
            host.Start();
        }

        public void Dispose()
        {
            host.Stop();
            host.Dispose();
        }
    }
}
