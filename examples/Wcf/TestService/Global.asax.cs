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

            builder.Register<TestService.Service1>().As<TestService.IService1>().WithScope(Autofac.InstanceScope.Factory);
            builder.Register<Test>().As<ITest>();

            Autofac.Integration.Wcf.AutofacServiceHostFactory.Container = builder.Build();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}