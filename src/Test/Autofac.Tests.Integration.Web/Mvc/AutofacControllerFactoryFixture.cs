using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Builder;
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

//        class StubContext : HttpContextBase { }
        
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
        	
			public IContainer RequestContainer
			{
				get
				{
					return _container;
				}
			}
        	
			public void DisposeRequestContainer()
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

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectsNullControllerName()
        {
            var target = CreateTarget();
            target.CreateController(CreateContext(), null);
        }

        [Test]
        [ExpectedException(typeof(HttpException))]
        public void ThrowsHttpException404WhenControllerMissing()
        {
            var target = CreateTarget();
            target.CreateController(CreateContext(), "NotAController");
        }

        private RequestContext CreateContext()
        {
            var request = new Mock<HttpRequestBase>(MockBehavior.Loose);
            request.Expect(r => r.Path).Returns("Path");
            var httpContext = new Mock<HttpContextBase>(MockBehavior.Loose);
            httpContext.ExpectGet(c => c.Request).Returns(request.Object);
            return new RequestContext(httpContext.Object, new RouteData());
        }

        [Test]
        public void CreatesController()
        {
            var builder = new ContainerBuilder();
            builder.Register<StubController>()
            	.FactoryScoped()
            	.Named("controller." + HomeControllerName.ToLowerInvariant());
            var container = builder.Build();

            var context = CreateContext();

            var provider = new StubContainerProvider(container);
            var target = new AutofacControllerFactory(provider);
            var controller = target.CreateController(context, HomeControllerName);

            Assert.IsNotNull(controller);
            Assert.IsInstanceOfType(typeof(StubController), controller);
        }
    }
}
