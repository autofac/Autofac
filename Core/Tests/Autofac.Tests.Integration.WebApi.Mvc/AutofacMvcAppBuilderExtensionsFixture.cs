using System.Web;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Owin;
using OwinExtensions = Owin.AutofacMvcAppBuilderExtensions;

namespace Autofac.Tests.Integration.Mvc.Owin
{
    [TestFixture]
    public class AutofacMvcAppBuilderExtensionsFixture
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