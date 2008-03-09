using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Integration.Web.Mvc;
using System.Web.Mvc;
using System.Web;
using Autofac.Builder;
using System.Web.Routing;
using Autofac.Integration.Web;

namespace Autofac.Tests.Integration.Web.Mvc
{
    [TestFixture]
    public class AutofacControllerFactoryFixture
    {
        class StubController : Controller { }

        class StubContext : HttpContextBase { }
        
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
            return new AutofacControllerFactory(new ContainerProvider(new Container()));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DetectsNullControllerName()
        {
            var target = CreateTarget();
            target.CreateController(CreateContext(), null);
        }

        private RequestContext CreateContext()
        {
            HttpContextBase httpContext = new StubContext();
            return new RequestContext(httpContext, new RouteData());
        }

        [Test]
        public void CreatesController()
        {
            var builder = new ContainerBuilder();
            builder.Register<StubController>()
            	.FactoryScoped()
            	.Named("controller." + HomeControllerName.ToLowerInvariant());
            var container = builder.Build();

            var httpContext = new StubContext();
            var context = new RequestContext(httpContext, new RouteData());

            var provider = new StubContainerProvider(container);
            var target = new AutofacControllerFactory(provider);
            var controller = target.CreateController(context, HomeControllerName);

            Assert.IsNotNull(controller);
            Assert.IsInstanceOfType(typeof(StubController), controller);
        }
    }
}
