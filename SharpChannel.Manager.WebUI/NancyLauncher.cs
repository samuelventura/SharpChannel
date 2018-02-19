using System;
using Nancy.Hosting.Self;
using System.Configuration;

namespace SharpChannel.Manager.WebUI
{
    public class NancyLauncher : IDisposable
    {
        private readonly NancyHost host;
        private readonly string uri;

        public string URI { get { return uri; } }

        public NancyLauncher()
        {
            var port = ConfigurationManager.AppSettings.Get("port");
            uri = string.Format("http://localhost:{0}", port);
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
