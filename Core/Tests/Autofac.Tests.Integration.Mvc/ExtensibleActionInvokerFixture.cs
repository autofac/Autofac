using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
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

            _controller = new TestController { ValidateRequest = false };
            _context = new ControllerContext { Controller = _controller, HttpContext = httpContext.Object };
            _controller.ControllerContext = _context;
            _controller.ValueProvider = new NameValueCollectionValueProvider(new NameValueCollection(), CultureInfo.InvariantCulture);
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
            this.SetChildAction();
            var invoker = _container.Resolve<TestableActionInvoker>(new NamedParameter("injectActionMethodParameters", true));
            invoker.InvokeAction(_context, "Index");

            Assert.That(_controller.Dependency, Is.InstanceOf<IActionDependency>());
        }

        [Test]
        public void ActionInjectionTurnedOff_DependencyRegistered_ServiceNotResolved()
        {
            /* If action injection is turned off, this falls back to a
             * default value by attempting to bind a model using the
             * default model binder. That is...
             * - ExtensibleActionInvoker.GetParameterValue falls back to
             * - ControllerActionInvoker.GetParameterValue, which uses the
             *   incoming ParameterDescriptor to attempt to model bind to a value.
             * - DefaultModelBinder.BindModel looks for a value being passed in
             *   and, not finding one, calls Activator.CreateInstance on the expected
             *   dependency type. If the dependency type is an interface, an exception
             *   is raised because you can't create instances of interfaces.
             */
            this.SetChildAction();
            var invoker = _container.Resolve<TestableActionInvoker>();
            invoker.InvokeAction(_context, "Index");

            Assert.That(_controller.Dependency, Is.Null);
        }

        private void SetChildAction()
        {
            // Setting ParentActionViewContext makes this a "child action" and
            // side-steps the call for DynamicValidationShim.EnableDynamicValidation(HttpRequest)
            // which isn't mockable, requires a real HttpContext.Current, and
            // throws NullReferenceException if the HttpContext.Current.Request doesn't exist.
            _context.RouteData.DataTokens["ParentActionViewContext"] = "parent";
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
