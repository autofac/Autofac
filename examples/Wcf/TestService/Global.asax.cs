using System;
using Autofac.Builder;
using TestService;

namespace InjectedService
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.Register<TestService.Service1>().WithScope(Autofac.InstanceScope.Factory);
            builder.Register<Test>().As<ITest>();

            Autofac.Integration.Wcf.AutofacServiceHostFactory.Container = builder.Build();
        }
    }
}