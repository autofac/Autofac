using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac.Integration.Web.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Web.Mvc
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
            _container = builder.Build();

            _controller = new TestController();
            _context = new ControllerContext { Controller = _controller };
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
                IEnumerable<IResultFilter> resultFilters)
                : base(context, actionFilters, authorizationFilters, exceptionFilters, resultFilters)
            {
            }

            public FilterInfo GetFiltersPublic()
            {
                var controllerDescriptor = new ReflectedControllerDescriptor(typeof(TestController));
                var actionDescriptor = controllerDescriptor.GetCanonicalActions().Single(x => x.ActionName == "Index");
                return base.GetFilters(new ControllerContext(), actionDescriptor);
            }
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
            [TestResultFilter, TestActionFilter, TestExceptionFilter, TestAuthFilter]
            public ActionResult Index()
            {
                return null;
            }
        }

    }
}
