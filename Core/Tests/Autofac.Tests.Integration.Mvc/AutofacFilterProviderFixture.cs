using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class AutofacFilterProviderFixture
    {
        ControllerContext _baseControllerContext;
        ControllerContext _derivedControllerContext;
        ControllerContext _mostDerivedControllerContext;
        ControllerDescriptor _controllerDescriptor;

        MethodInfo _baseMethodInfo;
        MethodInfo _derivedMethodInfo;
        MethodInfo _mostDerivedMethodInfo;
        string _actionName;

        ReflectedActionDescriptor _reflectedActionDescriptor;
        ReflectedAsyncActionDescriptor _reflectedAsyncActionDescriptor;
        TaskAsyncActionDescriptor _taskAsyncActionDescriptor;
        ReflectedActionDescriptor _derivedActionDescriptor;
        ReflectedActionDescriptor _mostDerivedActionDescriptor;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _baseControllerContext = new ControllerContext {Controller = new TestController()};
            _derivedControllerContext = new ControllerContext {Controller = new TestControllerA()};
            _mostDerivedControllerContext = new ControllerContext {Controller = new TestControllerB()};

            _baseMethodInfo = TestController.GetAction1MethodInfo<TestController>();
            _derivedMethodInfo = TestController.GetAction1MethodInfo<TestControllerA>();
            _mostDerivedMethodInfo = TestController.GetAction1MethodInfo<TestControllerB>();
            _actionName = _baseMethodInfo.Name;

            _controllerDescriptor = new Mock<ControllerDescriptor>().Object;
            _reflectedActionDescriptor = new ReflectedActionDescriptor(_baseMethodInfo, _actionName, _controllerDescriptor);
            _reflectedAsyncActionDescriptor = new ReflectedAsyncActionDescriptor(_baseMethodInfo, _baseMethodInfo, _actionName, _controllerDescriptor);
            _taskAsyncActionDescriptor = new TaskAsyncActionDescriptor(_baseMethodInfo, _actionName, _controllerDescriptor);
            _derivedActionDescriptor = new ReflectedActionDescriptor(_derivedMethodInfo, _actionName, _controllerDescriptor);
            _mostDerivedActionDescriptor = new ReflectedActionDescriptor(_mostDerivedMethodInfo, _actionName, _controllerDescriptor);
        }

        [Test]
        public void AsActionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestActionFilter()).AsActionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedActionFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter<TestActionFilter>(
                FilterScope.Controller,
                _reflectedActionDescriptor,
                r => r.AsActionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedActionFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter<TestActionFilter>(
                FilterScope.Action,
                _reflectedActionDescriptor,
                r => r.AsActionFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedActionFilterForReflectedAsyncActionDescriptor()
        {
            AssertSingleFilter<TestActionFilter>(
                FilterScope.Action,
                _reflectedAsyncActionDescriptor,
                r => r.AsActionFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedActionFilterForTaskAsyncActionDescriptor()
        {
            AssertSingleFilter<TestActionFilter>(
                FilterScope.Action,
                _taskAsyncActionDescriptor,
                r => r.AsActionFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedActionFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestActionFilter>(
                FilterScope.Action,
                _derivedActionDescriptor,
                r => r.AsActionFilterFor<TestController>(c => c.Action1(default(string))),
                _derivedControllerContext);
        }

        [Test]
        public void ResolvesActionScopedActionFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestActionFilter>(
                FilterScope.Action,
                _mostDerivedActionDescriptor,
                r => r.AsActionFilterFor<TestController>(c => c.Action1(default(string))),
                _mostDerivedControllerContext);
        }

        [Test]
        public void ResolvesMultipleControllerScopedActionFilters()
        {
            AssertMultipleFilters<TestActionFilter, TestActionFilter2>(
                FilterScope.Controller,
                r => r.AsActionFilterFor<TestController>(),
                r => r.AsActionFilterFor<TestController>(20));
        }

        [Test]
        public void ResolvesMultipleActionScopedActionFilters()
        {
            AssertMultipleFilters<TestActionFilter, TestActionFilter2>(
                FilterScope.Action,
                r => r.AsActionFilterFor<TestController>(c => c.Action1(default(string))),
                r => r.AsActionFilterFor<TestController>(c => c.Action1(default(string)), 20));
        }

        [Test]
        public void AsAuthorizationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestAuthorizationFilter()).AsAuthorizationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedAuthorizationFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter<TestAuthorizationFilter>(
                FilterScope.Controller,
                _reflectedActionDescriptor,
                r => r.AsAuthorizationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter<TestAuthorizationFilter>(
                FilterScope.Action,
                _reflectedActionDescriptor,
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterForReflectedAsyncActionDescriptor()
        {
            AssertSingleFilter<TestAuthorizationFilter>(
                FilterScope.Action,
                _reflectedAsyncActionDescriptor,
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterForTaskAsyncActionDescriptor()
        {
            AssertSingleFilter<TestAuthorizationFilter>(
                FilterScope.Action,
                _taskAsyncActionDescriptor,
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestAuthorizationFilter>(
                FilterScope.Action,
                _derivedActionDescriptor,
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string))),
                _derivedControllerContext);
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestAuthorizationFilter>(
                FilterScope.Action,
                _mostDerivedActionDescriptor,
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string))),
                _mostDerivedControllerContext);
        }

        [Test]
        public void ResolvesMultipleControllerScopedAuthorizationFilters()
        {
            AssertMultipleFilters<TestAuthorizationFilter, TestAuthorizationFilter2>(
                FilterScope.Controller,
                r => r.AsAuthorizationFilterFor<TestController>(),
                r => r.AsAuthorizationFilterFor<TestController>(20));
        }

        [Test]
        public void ResolvesMultipleActionScopedAuthorizationFilters()
        {
            AssertMultipleFilters<TestAuthorizationFilter, TestAuthorizationFilter2>(
                FilterScope.Action,
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string))),
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Action1(default(string)), 20));
        }

        [Test]
        public void AsExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestExceptionFilter()).AsExceptionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedExceptionFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter<TestExceptionFilter>(
                FilterScope.Controller,
                _reflectedActionDescriptor,
                r => r.AsExceptionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter<TestExceptionFilter>(
                FilterScope.Action,
                _reflectedActionDescriptor,
                r => r.AsExceptionFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterForReflectedAsyncActionDescriptor()
        {
            AssertSingleFilter<TestExceptionFilter>(
                FilterScope.Action,
                _reflectedAsyncActionDescriptor,
                r => r.AsExceptionFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterForTaskAsyncActionDescriptor()
        {
            AssertSingleFilter<TestExceptionFilter>(
                FilterScope.Action,
                _taskAsyncActionDescriptor,
                r => r.AsExceptionFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestExceptionFilter>(
                FilterScope.Action,
                _derivedActionDescriptor,
                r => r.AsExceptionFilterFor<TestController>(c => c.Action1(default(string))),
                _derivedControllerContext);
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestExceptionFilter>(
                FilterScope.Action,
                _mostDerivedActionDescriptor,
                r => r.AsExceptionFilterFor<TestController>(c => c.Action1(default(string))),
                _mostDerivedControllerContext);
        }

        [Test]
        public void ResolvesMultipleControllerScopedExceptionFilters()
        {
            AssertMultipleFilters<TestExceptionFilter, TestExceptionFilter2>(
                FilterScope.Controller,
                r => r.AsExceptionFilterFor<TestController>(),
                r => r.AsExceptionFilterFor<TestController>(20));
        }

        [Test]
        public void ResolvesMultipleActionScopedExceptionFilters()
        {
            AssertMultipleFilters<TestExceptionFilter, TestExceptionFilter2>(
                FilterScope.Action,
                r => r.AsExceptionFilterFor<TestController>(c => c.Action1(default(string))),
                r => r.AsExceptionFilterFor<TestController>(c => c.Action1(default(string)), 20));
        }

        [Test]
        public void AsResultFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestResultFilter()).AsResultFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedResultFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter<TestResultFilter>(
                FilterScope.Controller,
                _reflectedActionDescriptor,
                r => r.AsResultFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedResultFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter<TestResultFilter>(
                FilterScope.Action,
                _reflectedActionDescriptor,
                r => r.AsResultFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedResultFilterForReflectedAsyncActionDescriptor()
        {
            AssertSingleFilter<TestResultFilter>(
                FilterScope.Action,
                _reflectedAsyncActionDescriptor,
                r => r.AsResultFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedResultFilterForTaskAsyncActionDescriptor()
        {
            AssertSingleFilter<TestResultFilter>(
                FilterScope.Action,
                _taskAsyncActionDescriptor,
                r => r.AsResultFilterFor<TestController>(c => c.Action1(default(string))));
        }

        [Test]
        public void ResolvesActionScopedResultFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestResultFilter>(
                FilterScope.Action,
                _derivedActionDescriptor,
                r => r.AsResultFilterFor<TestController>(c => c.Action1(default(string))),
                _derivedControllerContext);
        }

        [Test]
        public void ResolvesActionScopedResultFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestResultFilter>(
                FilterScope.Action,
                _mostDerivedActionDescriptor,
                r => r.AsResultFilterFor<TestController>(c => c.Action1(default(string))),
                _mostDerivedControllerContext);
        }

        [Test]
        public void ResolvesMultipleControllerScopedResultFilters()
        {
            AssertMultipleFilters<TestResultFilter, TestResultFilter2>(
                FilterScope.Controller,
                r => r.AsResultFilterFor<TestController>(),
                r => r.AsResultFilterFor<TestController>(20));
        }

        [Test]
        public void ResolvesMultipleActionScopedResultFilters()
        {
            AssertMultipleFilters<TestResultFilter, TestResultFilter2>(
                FilterScope.Action,
                r => r.AsResultFilterFor<TestController>(c => c.Action1(default(string))),
                r => r.AsResultFilterFor<TestController>(c => c.Action1(default(string)), 20));
        }

        static void SetupMockLifetimeScopeProvider(ILifetimeScope container)
        {
            var lifetimeScope = container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag);
            var lifetimeScopeProvider = new Mock<ILifetimeScopeProvider>();
            lifetimeScopeProvider.Setup(mock => mock.GetLifetimeScope()).Returns(lifetimeScope);
            var resolver = new AutofacDependencyResolver(container, lifetimeScopeProvider.Object);
            DependencyResolver.SetResolver(resolver);
        }

        void AssertSingleFilter<TFilter>(FilterScope filterScope, ActionDescriptor actionDescriptor,
            Action<IRegistrationBuilder<TFilter, SimpleActivatorData, SingleRegistrationStyle>> configure)
            where TFilter : new()
        {
            AssertSingleFilter(filterScope, actionDescriptor, configure, _baseControllerContext);
        }

        static void AssertSingleFilter<TFilter>(FilterScope filterScope, ActionDescriptor actionDescriptor,
            Action<IRegistrationBuilder<TFilter, SimpleActivatorData, SingleRegistrationStyle>> configure,
            ControllerContext controllerContext)
            where TFilter : new()
        {
            var builder = new ContainerBuilder();
            configure(builder.Register(c => new TFilter()));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TFilter>());
            Assert.That(filters[0].Scope, Is.EqualTo(filterScope));
        }

        void AssertMultipleFilters<TFilter1, TFilter2>(FilterScope filterScope,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure1,
            Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> configure2)
            where TFilter1 : new() where TFilter2 : new()
        {
            var builder = new ContainerBuilder();
            configure1(builder.Register(c => new TFilter1()));
            configure2(builder.Register(c => new TFilter2()));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var actionDescriptor = new ReflectedActionDescriptor(_baseMethodInfo, _actionName, _controllerDescriptor);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(_baseControllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(2));

            var filter = filters.Single(f => f.Instance is TFilter1);
            Assert.That(filter.Scope, Is.EqualTo(filterScope));
            Assert.That(filter.Order, Is.EqualTo(Filter.DefaultOrder));

            filter = filters.Single(f => f.Instance is TFilter2);
            Assert.That(filter.Scope, Is.EqualTo(filterScope));
            Assert.That(filter.Order, Is.EqualTo(20));
        }
    }
}
