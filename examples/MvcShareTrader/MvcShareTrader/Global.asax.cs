using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using MvcShareTrader.Components;
using MvcShareTrader.Models;
using MvcShareTrader.Services;

namespace MvcShareTrader
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Note: Change Url= to Url="[controller].mvc/[action]/[id]" to enable 
            //       automatic support on IIS6 

            RouteTable.Routes.Add(new Route
            {
                Url = "[controller]/[action]/[id]",
                Defaults = new { action = "Index", id = (string)null },
                RouteHandler = typeof(MvcRouteHandler)
            });

            RouteTable.Routes.Add(new Route
            {
                Url = "Default.aspx",
                Defaults = new { controller = "Home", action = "Index", id = (string)null },
                RouteHandler = typeof(MvcRouteHandler)
            });

            ContainerBuilder builder = new ContainerBuilder();

            builder.Register<Portfolio>()
                .WithScope(InstanceScope.Factory);

            builder.Register<Shareholding>()
                .WithScope(InstanceScope.Factory)
                .ThroughFactory<Shareholding.Factory>();

            builder.Register<WebQuoteService>()
                .As<IQuoteService>()
                .WithScope(InstanceScope.Container);

            foreach (Type controllerType in Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(IController).IsAssignableFrom(t)))
                builder.Register(controllerType).WithScope(InstanceScope.Factory);

            AutofacMvcIntegration.Install(builder.Build());
        }
    }
}