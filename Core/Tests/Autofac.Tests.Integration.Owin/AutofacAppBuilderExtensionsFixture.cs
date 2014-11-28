using System.Collections.Generic;
using System.Threading;
using Autofac.Core.Lifetime;
using Autofac.Integration.Owin;
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
            app.Setup(mock => mock.Properties).Returns(new Dictionary<string, object>());
            app.Setup(mock => mock.Use(typeof(AutofacMiddleware<TestMiddleware>)));

            OwinExtensions.UseAutofacMiddleware(app.Object, container);

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
                Assert.That(TestMiddleware.LifetimeScope.Tag, Is.EqualTo(MatchingScopeLifetimeTags.RequestLifetimeScopeTag));
            }
        }

        [Test]
        public void DisposeContainerOnAppDisposing()
        {
            var container = new Mock<ILifetimeScope>();
            var app = new Mock<IAppBuilder>();
            var cts = new CancellationTokenSource();
            var props = new Dictionary<string, object> {{Constants.OwinHostOnAppDisposingKey, cts.Token}};
            app.Setup(mock => mock.Properties).Returns(props);

            OwinExtensions.DisposeContainerOnAppDisposing(app.Object, container.Object);

            app.VerifyAll();

            cts.Cancel();

            container.Verify(c => c.Dispose(), Times.Once);
        }

        [Test]
        public void DisposeContainerOnAppDisposing_IgnoresDefaultToken()
        {
            var container = new Mock<ILifetimeScope>();
            var app = new Mock<IAppBuilder>();
            app.Setup(mock => mock.Properties).Returns(new Dictionary<string, object>());

            OwinExtensions.DisposeContainerOnAppDisposing(app.Object, container.Object);

            app.VerifyAll();

            container.Verify(c => c.Dispose(), Times.Never);
        }
    }
}
