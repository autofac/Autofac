using System;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Wcf;
using TestService;

namespace InjectedService
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Service1>().InstancePerDependency();
            builder.RegisterType<Test>().As<ITest>().InstancePerLifetimeScope();

            AutofacHostFactory.Container = builder.Build();
        }
    }
}