using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using Autofac.Integration.Mvc;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class AutofacFilterProviderFixture
    {
        ControllerContext _controllerContext;
        ControllerDescriptor _controllerDescriptor;
        MethodInfo _methodInfo;
        string _actionName;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var controllerContext = new Mock<ControllerContext>();
            controllerContext.Setup(mock => mock.Controller).Returns(new TestController());
            _controllerContext = controllerContext.Object;
            _controllerDescriptor = new Mock<ControllerDescriptor>().Object;
            _methodInfo = TestController.GetAction1MethodInfo();
            _actionName = _methodInfo.Name;
        }

        [Test]
        public void ResolvesControllerScopedActionFilterFromReflectedActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestActionFilter()).AsActionFilterFor<TestController>();
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestActionFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Controller));
        }

        [Test]
        public void ResolvesActionScopedActionFilterFromReflectedActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestActionFilter()).AsActionFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestActionFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesActionScopedActionFilterFromReflectedAsyncActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestActionFilter()).AsActionFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedAsyncActionDescriptor(_methodInfo, _methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestActionFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesActionScopedActionFilterFromTaskAsyncActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestActionFilter()).AsActionFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new TaskAsyncActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestActionFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesMultipleControllerScopedActionFilters()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestActionFilter()).AsActionFilterFor<TestController>();
            builder.Register(c => new TestActionFilter2()).AsActionFilterFor<TestController>().WithFilterOrder(20);
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(2));

            var filter = filters.Single(f => f.Instance is TestActionFilter);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Controller));
            Assert.That(filter.Order, Is.EqualTo(Filter.DefaultOrder));

            filter = filters.Single(f => f.Instance is TestActionFilter2);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Controller));
            Assert.That(filter.Order, Is.EqualTo(20));
        }

        [Test]
        public void ResolvesMultipleActionScopedActionFilters()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestActionFilter()).AsActionFilterFor<TestController>(c => c.Action1(default(string)));
            builder.Register(c => new TestActionFilter2()).AsActionFilterFor<TestController>(c => c.Action1(default(string))).WithFilterOrder(20);
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(2));

            var filter = filters.Single(f => f.Instance is TestActionFilter);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Action));
            Assert.That(filter.Order, Is.EqualTo(Filter.DefaultOrder));

            filter = filters.Single(f => f.Instance is TestActionFilter2);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Action));
            Assert.That(filter.Order, Is.EqualTo(20));
        }

        [Test]
        public void ResolvesControllerScopedAuthorizationFilterFromReflectedActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestAuthorizationFilter()).AsAuthorizationFilterFor<TestController>();
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestAuthorizationFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Controller));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterFromReflectedActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestAuthorizationFilter()).AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestAuthorizationFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterFromReflectedAsyncActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestAuthorizationFilter()).AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedAsyncActionDescriptor(_methodInfo, _methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestAuthorizationFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterFromTaskAsyncActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestAuthorizationFilter()).AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new TaskAsyncActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestAuthorizationFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesMultipleControllerScopedAuthorizationFilters()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestAuthorizationFilter()).AsAuthorizationFilterFor<TestController>();
            builder.Register(c => new TestAuthorizationFilter2()).AsAuthorizationFilterFor<TestController>().WithFilterOrder(20);
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(2));

            var filter = filters.Single(f => f.Instance is TestAuthorizationFilter);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Controller));
            Assert.That(filter.Order, Is.EqualTo(Filter.DefaultOrder));

            filter = filters.Single(f => f.Instance is TestAuthorizationFilter2);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Controller));
            Assert.That(filter.Order, Is.EqualTo(20));
        }

        [Test]
        public void ResolvesMultipleActionScopedAuthorizationFilters()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestAuthorizationFilter()).AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string)));
            builder.Register(c => new TestAuthorizationFilter2()).AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string))).WithFilterOrder(20);
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(2));

            var filter = filters.Single(f => f.Instance is TestAuthorizationFilter);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Action));
            Assert.That(filter.Order, Is.EqualTo(Filter.DefaultOrder));

            filter = filters.Single(f => f.Instance is TestAuthorizationFilter2);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Action));
            Assert.That(filter.Order, Is.EqualTo(20));
        }

        [Test]
        public void ResolvesControllerScopedExceptionFilterFromReflectedActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestExceptionFilter()).AsExceptionFilterFor<TestController>();
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestExceptionFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Controller));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterFromReflectedActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestExceptionFilter()).AsExceptionFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestExceptionFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterFromReflectedAsyncActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestExceptionFilter()).AsExceptionFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedAsyncActionDescriptor(_methodInfo, _methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestExceptionFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterFromTaskAsyncActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestExceptionFilter()).AsExceptionFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new TaskAsyncActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestExceptionFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesMultipleControllerScopedExceptionFilters()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestExceptionFilter()).AsExceptionFilterFor<TestController>();
            builder.Register(c => new TestExceptionFilter2()).AsExceptionFilterFor<TestController>().WithFilterOrder(20);
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(2));

            var filter = filters.Single(f => f.Instance is TestExceptionFilter);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Controller));
            Assert.That(filter.Order, Is.EqualTo(Filter.DefaultOrder));

            filter = filters.Single(f => f.Instance is TestExceptionFilter2);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Controller));
            Assert.That(filter.Order, Is.EqualTo(20));
        }

        [Test]
        public void ResolvesMultipleActionScopedExceptionFilters()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestExceptionFilter()).AsExceptionFilterFor<TestController>(c => c.Action1(default(string)));
            builder.Register(c => new TestExceptionFilter2()).AsExceptionFilterFor<TestController>(c => c.Action1(default(string))).WithFilterOrder(20);
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(2));

            var filter = filters.Single(f => f.Instance is TestExceptionFilter);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Action));
            Assert.That(filter.Order, Is.EqualTo(Filter.DefaultOrder));

            filter = filters.Single(f => f.Instance is TestExceptionFilter2);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Action));
            Assert.That(filter.Order, Is.EqualTo(20));
        }


        [Test]
        public void ResolvesControllerScopedResultFilterFromReflectedActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestResultFilter()).AsResultFilterFor<TestController>();
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestResultFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Controller));
        }

        [Test]
        public void ResolvesActionScopedResultFilterFromReflectedActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestResultFilter()).AsResultFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestResultFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesActionScopedResultFilterFromReflectedAsyncActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestResultFilter()).AsResultFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedAsyncActionDescriptor(_methodInfo, _methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestResultFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesActionScopedResultFilterFromTaskAsyncActionDescriptor()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestResultFilter()).AsResultFilterFor<TestController>(c => c.Action1(default(string)));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new TaskAsyncActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TestResultFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(FilterScope.Action));
        }

        [Test]
        public void ResolvesMultipleControllerScopedReultFilters()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestResultFilter()).AsResultFilterFor<TestController>();
            builder.Register(c => new TestResultFilter2()).AsResultFilterFor<TestController>().WithFilterOrder(20);
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(2));

            var filter = filters.Single(f => f.Instance is TestResultFilter);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Controller));
            Assert.That(filter.Order, Is.EqualTo(Filter.DefaultOrder));

            filter = filters.Single(f => f.Instance is TestResultFilter2);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Controller));
            Assert.That(filter.Order, Is.EqualTo(20));
        }

        [Test]
        public void ResolvesMultipleActionScopedReultFilters()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new TestResultFilter()).AsResultFilterFor<TestController>(c => c.Action1(default(string)));
            builder.Register(c => new TestResultFilter2()).AsResultFilterFor<TestController>(c => c.Action1(default(string))).WithFilterOrder(20);
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_methodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(2));

            var filter = filters.Single(f => f.Instance is TestResultFilter);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Action));
            Assert.That(filter.Order, Is.EqualTo(Filter.DefaultOrder));

            filter = filters.Single(f => f.Instance is TestResultFilter2);
            Assert.That(filter.Scope, Is.EqualTo(FilterScope.Action));
            Assert.That(filter.Order, Is.EqualTo(20));
        }

        static void SetupMockLifetimeScopeProvider(ILifetimeScope container)
        {
            var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag);
            var lifetimeScopeProvider = new Mock<ILifetimeScopeProvider>();
            lifetimeScopeProvider.Setup(mock => mock.GetLifetimeScope()).Returns(lifetimeScope);
            var resolver = new AutofacDependencyResolver(container, lifetimeScopeProvider.Object);
            DependencyResolver.SetResolver(resolver);
        }
    }
}
