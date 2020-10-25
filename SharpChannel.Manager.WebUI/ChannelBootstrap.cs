using System;
using Nancy;
using Nancy.Conventions;
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

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.ViewLocationConventions.Clear();
            nancyConventions.ViewLocationConventions.Add((viewName, model, context) => string.Concat("views/thirdparty/", viewName));
            nancyConventions.ViewLocationConventions.Add((viewName, model, context) => string.Concat("views/", viewName));
        }
    }
}
