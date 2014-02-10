using System.Web;
using Autofac.Integration.Mvc.Owin;
using Autofac.Integration.Owin;
using Autofac.Tests.Integration.Owin;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Owin;
using OwinExtensions = Autofac.Integration.Mvc.Owin.OwinExtensions;

namespace Autofac.Tests.Integration.Mvc.Owin
{
    [TestFixture]
    public class OwinExtensionsFixture
    {
        [Test]
        public void UseAutofacMvcUpdatesHttpContextWithLifetimeScopeFromOwinContext()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestMiddleware>();
            var container = builder.Build();

            var httpContext = new Mock<HttpContextBase>();
            httpContext.SetupSet(mock => mock.Items[typeof(ILifetimeScope)] = It.IsAny<ILifetimeScope>());
            OwinExtensions.CurrentHttpContext = () => httpContext.Object;

            using (var server = TestServer.Create(app =>
            {
                app.UseAutofacMiddleware(container);
                app.UseAutofacMvc();
                app.Run(context => context.Response.WriteAsync("Hello, world!"));
            }))
            {
                server.HttpClient.GetAsync("/").Wait();
                httpContext.VerifyAll();
            }
        }
    }
}