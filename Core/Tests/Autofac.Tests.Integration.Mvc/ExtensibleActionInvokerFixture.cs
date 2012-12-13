using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class ExtensibleActionInvokerFixture
    {
        IContainer _container;
        ControllerContext _context;
        TestController _controller;
        IDependencyResolver _originalResolver;

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ExtensibleActionInvoker>().As<IActionInvoker>();
            builder.Register(c => new ActionDependency()).As<IActionDependency>();
            _container = builder.Build();

            this._originalResolver = DependencyResolver.Current;
            DependencyResolver.SetResolver(new AutofacDependencyResolver(_container, new StubLifetimeScopeProvider(_container)));

            var request = new Mock<HttpRequestBase>();
            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(mock => mock.Request).Returns(request.Object);

            _controller = new TestController { ValidateRequest = false };
            _context = new ControllerContext { Controller = _controller, HttpContext = httpContext.Object };
            _controller.ControllerContext = _context;
            _controller.ValueProvider = new NameValueCollectionValueProvider(new NameValueCollection(), CultureInfo.InvariantCulture);
        }

        [TearDown]
        public void TearDown()
        {
            DependencyResolver.SetResolver(this._originalResolver);
        }

        [Test]
        public void ActionInjection_DependencyRegistered_ServiceResolved()
        {
            var invoker = _container.Resolve<IActionInvoker>();
            invoker.InvokeAction(_context, "Index");

            Assert.That(_controller.Dependency, Is.InstanceOf<IActionDependency>());
        }

        private interface IActionDependency
        {
        }

        private class ActionDependency : IActionDependency
        {
        }

        private class TestController : Controller
        {
            public IActionDependency Dependency { get; private set; }

            // ReSharper disable UnusedMember.Local
            public ActionResult Index(IActionDependency dependency)
            // ReSharper restore UnusedMember.Local
            {
                Dependency = dependency;
                return null;
            }
        }
    }
}
