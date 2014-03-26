using System.Security;
using System.Threading.Tasks;
using Autofac.Integration.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using Moq;
using NUnit.Framework;
using Owin;

namespace Autofac.Tests.Integration.Owin
{
    [TestFixture]
    public class AutofacAppBuilderExtensionsFixture
    {
        [Test]
        public void UseAutofacMiddlewareAddsWrappedMiddlewareInstancesToAppBuilder()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestMiddleware>();
            var container = builder.Build();
            var app = new Mock<IAppBuilder>();
            app.Setup(mock => mock.Use(typeof(AutofacMiddleware<TestMiddleware>)));

            app.Object.UseAutofacMiddleware(container);

            app.VerifyAll();
        }

        [Test]
        public void UseAutofacMiddlewareAddsChildLifetimeScopeToOwinContext()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestMiddleware>();
            var container = builder.Build();

            using (var server = TestServer.Create(app =>
            {
                app.UseAutofacMiddleware(container);
                app.Run(context => context.Response.WriteAsync("Hello, world!"));
            }))
            {
                server.HttpClient.GetAsync("/").Wait();
                Assert.That(TestMiddleware.LifetimeScope.Tag, Is.EqualTo(Constants.LifetimeScopeTag));
            }
        }
    }

    public class TestMiddleware : OwinMiddleware
    {
        public static ILifetimeScope LifetimeScope { get; set; }

        public TestMiddleware(OwinMiddleware next)
            : base(next)
        {
            LifetimeScope = null;
        }

        [SecurityCritical]
        public override Task Invoke(IOwinContext context)
        {
            LifetimeScope = context.GetAutofacLifetimeScope();
            return Next.Invoke(context);
        }
    }
}
