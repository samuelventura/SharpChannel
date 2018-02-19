using System;
using Nancy;
using Nancy.TinyIoc;

namespace SharpChannel.Manager.WebUI
{
    public class ChannelBootstrap : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register<CachedManager>(new CachedManager());
        }
    }
}
