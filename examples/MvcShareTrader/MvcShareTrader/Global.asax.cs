using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using MvcShareTrader.Components;
using MvcShareTrader.Models;
using MvcShareTrader.Services;

namespace MvcShareTrader
{
    public class Global : System.Web.HttpApplication, IContainerProviderAccessor
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes(RouteTable.Routes);
            
            var builder = new ContainerBuilder();

            builder.RegisterType<Portfolio>().InstancePerDependency();

            builder.RegisterType<Shareholding>();
            builder.RegisterGeneratedFactory<Shareholding.Factory>();

            builder.RegisterType<WebQuoteService>()
                .As<IQuoteService>()
                .InstancePerLifetimeScope();

            builder.RegisterModule(new AutofacControllerModule(Assembly.GetExecutingAssembly()));

            _containerProvider = new ContainerProvider(builder.Build());
            ControllerBuilder.Current.SetControllerFactory(new AutofacControllerFactory(ContainerProvider));            
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            ContainerProvider.EndRequestLifetime();
        }

        protected static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Home", action = "Index", id = "" }  // Parameter defaults
            );
        }

        private static IContainerProvider _containerProvider;
        public IContainerProvider ContainerProvider
        {
            get { return _containerProvider; }
        }


    }
}