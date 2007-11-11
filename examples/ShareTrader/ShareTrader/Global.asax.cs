using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;
using Autofac;
using Autofac.Builder;
using ShareTrader.Model;
using ShareTrader.Components;
using ShareTrader.Services;

namespace ShareTrader
{
    public class Global : System.Web.HttpApplication
    {
        static Container OuterContainer { get; set; }

        Container RequestContainer { get; set; }

        public static Container Container
        {
            get
            {
                return ((Global)HttpContext.Current.ApplicationInstance).RequestContainer;
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            ContainerBuilder builder = new ContainerBuilder();

            using (builder.SetDefaultScope(InstanceScope.Factory))
            {
                builder.Register<Portfolio>();
                builder.Register<Shareholding>()
                  .ThroughFactory<Shareholding.Factory>();
            }

            builder.Register<WebQuoteService>()
                .As<IQuoteService>()
                .WithScope(InstanceScope.Container);

            OuterContainer = builder.Build();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            RequestContainer = OuterContainer.CreateInnerContainer();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            RequestContainer.Dispose();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            OuterContainer.Dispose();
        }
    }
}