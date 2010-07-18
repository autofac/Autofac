using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using NUnit.Framework;
using Moq;

namespace Autofac.Tests.Integration.Web.Mvc
{
    [TestFixture]
    public class AutofacControllerFactoryFixture
    {
        class StubController : Controller { }

        class StubContainerProvider : IContainerProvider
        {
            readonly IContainer _container;

            public StubContainerProvider(IContainer container)
            {
                _container = container;
            }

            public IContainer ApplicationContainer
            {
                get
                {
                    return _container;
                }
            }

            public ILifetimeScope RequestLifetime
            {
                get
                {
                    return _container;
                }
            }

            public void EndRequestLifetime()
            {
            }
        }

        const string HomeControllerName = "Home";

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectsNullContext()
        {
            var target = CreateTarget();
            target.CreateController(null, HomeControllerName);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectsNullProvider()
        {
            new AutofacControllerFactory(null);
        }

        private static AutofacControllerFactory CreateTarget()
        {
            return new AutofacControllerFactory(new StubContainerProvider(new ContainerBuilder().Build()));
        }

        [Test]
        public void ThrowsHttpException404WhenControllerIsNull()
        {
            var target = CreateTarget();
            var hx = Assert.Throws<HttpException>(() => 
                target.CreateControllerInternal(CreateContext(), null));
            Assert.That(hx.GetHttpCode(), Is.EqualTo(404));
        }

        [Test]
        public void ThrowsHttpException404WhenControllerMissing()
        {
            var target = CreateTarget();
            var hx = Assert.Throws<HttpException>(() =>
                target.CreateControllerInternal(CreateContext(), typeof(object)));
            Assert.That(hx.GetHttpCode(), Is.EqualTo(404));
        }

        private static RequestContext CreateContext()
        {
            var request = new Mock<HttpRequestBase>(MockBehavior.Loose);
            request.Setup(r => r.Path).Returns("Path");
            var httpContext = new Mock<HttpContextBase>(MockBehavior.Loose);
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            return new RequestContext(httpContext.Object, new RouteData());
        }

        [Test]
        public void CreatesController()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubController>();
            var container = builder.Build();

            var context = CreateContext();

            var provider = new StubContainerProvider(container);
            var target = new AutofacControllerFactory(provider);
            var controller = target.CreateControllerInternal(context, typeof(StubController));

            Assert.IsNotNull(controller);
            Assert.IsInstanceOf<StubController>(controller);
        }
    }
}
