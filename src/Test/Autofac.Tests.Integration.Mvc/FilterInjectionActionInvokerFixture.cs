using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<TestableActionInvoker>();
            builder.Register(c => new TestDependency());
            builder.Register(c => new ActionDependency()).As<IActionDependency>();
            _container = builder.Build();

            var request = new Mock<HttpRequestBase>();
            var httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(mock => mock.Request).Returns(request.Object);

            _controller = new TestController();
            _context = new ControllerContext {Controller = _controller, HttpContext = httpContext.Object};
            _controller.ControllerContext = _context;
        }

        [Test]
        public void InjectsFilters()
        {
            var invoker = _container.Resolve<TestableActionInvoker>();
            var filters = invoker.GetFiltersPublic();
            AssertFiltersInjected(filters.ActionFilters);
            AssertFiltersInjected(filters.AuthorizationFilters);
            AssertFiltersInjected(filters.ExceptionFilters);
            AssertFiltersInjected(filters.ResultFilters);
        }

        [Test]
        public void ActionInjectionTurnedOn_DependencyRegistered_ServiceResolved()
        {
            var invoker = _container.Resolve<TestableActionInvoker>(new NamedParameter("injectActionMethodParameters", true));
            invoker.InvokeAction(_context, "Index");

            Assert.That(_controller.Dependency, Is.InstanceOf<IActionDependency>());
        }

        [Test, Ignore("Controller context (base) failing in fixture.")]
        public void ActionInjectionTurnedOff_DependencyRegistered_ServiceNotResolved()
        {
            var invoker = _container.Resolve<TestableActionInvoker>();
            invoker.InvokeAction(_context, "Index");

            Assert.That(_controller.Dependency, Is.Null);
        }

        private static void AssertFiltersInjected(IEnumerable filters)
        {
            foreach (var filter in filters.Cast<IHasDependency>())
            {
                Assert.IsNotNull(filter.Dependency);
            }
        }

// ReSharper disable ClassNeverInstantiated.Local
        private class TestableActionInvoker : ExtensibleActionInvoker
// ReSharper restore ClassNeverInstantiated.Local
        {
            public TestableActionInvoker(IComponentContext context,
                IEnumerable<IActionFilter> actionFilters,
                IEnumerable<IAuthorizationFilter> authorizationFilters,
                IEnumerable<IExceptionFilter> exceptionFilters,
                IEnumerable<IResultFilter> resultFilters,
                bool injectActionMethodParameters = false)
                : base(context, actionFilters, authorizationFilters, exceptionFilters, resultFilters, injectActionMethodParameters)
            {
            }

            public FilterInfo GetFiltersPublic()
            {
                var controllerDescriptor = new ReflectedControllerDescriptor(typeof(TestController));
                var actionDescriptor = controllerDescriptor.GetCanonicalActions().Single(x => x.ActionName == "Index");
                return base.GetFilters(new ControllerContext(), actionDescriptor);
            }
        }

        private interface IActionDependency
        {
        }

        private class ActionDependency : IActionDependency
        {
        }

        private class TestDependency
        {
        }

        private interface IHasDependency
        {
            TestDependency Dependency { get; }
        }

        private class TestActionFilter : ActionFilterAttribute, IHasDependency
        {
// ReSharper disable UnusedAutoPropertyAccessor.Local
            public TestDependency Dependency { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
            }
        }

        private class TestAuthFilter : FilterAttribute, IAuthorizationFilter, IHasDependency
        {
// ReSharper disable UnusedAutoPropertyAccessor.Local
            public TestDependency Dependency { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local

            public void OnAuthorization(AuthorizationContext filterContext)
            {
            }
        }

        private class TestResultFilter : FilterAttribute, IResultFilter, IHasDependency
        {
// ReSharper disable UnusedAutoPropertyAccessor.Local
            public TestDependency Dependency { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local

            public void OnResultExecuting(ResultExecutingContext filterContext)
            {
            }

            public void OnResultExecuted(ResultExecutedContext filterContext)
            {
            }
        }

        private class TestExceptionFilter : FilterAttribute, IExceptionFilter, IHasDependency
        {
// ReSharper disable UnusedAutoPropertyAccessor.Local
            public TestDependency Dependency { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local

            public void OnException(ExceptionContext filterContext)
            {
            }
        }

        private class TestController : Controller
        {
            public IActionDependency Dependency { get; private set; }

            [TestResultFilter, TestActionFilter, TestExceptionFilter, TestAuthFilter]
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
