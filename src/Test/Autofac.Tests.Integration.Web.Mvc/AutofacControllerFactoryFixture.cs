using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Builder;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using NUnit.Framework;
using Moq;
using Autofac.Core;

namespace Autofac.Tests.Integration.Web.Mvc
{
    [TestFixture]
    public class AutofacControllerFactoryFixture
    {
        class StubController : Controller { }

        class StubContainerProvider : IContainerProvider
        {
            IContainer _container;

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
            var target = new AutofacControllerFactory(null);
        }

        private AutofacControllerFactory CreateTarget()
        {
            return new AutofacControllerFactory(new StubContainerProvider(new Container()));
        }




        private RequestContext CreateContext()
        {
            var request = new Mock<HttpRequestBase>(MockBehavior.Loose);
            request.Setup(r => r.Path).Returns("Path");
            var httpContext = new Mock<HttpContextBase>(MockBehavior.Loose);
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            return new RequestContext(httpContext.Object, new RouteData());
        }



    }
}
