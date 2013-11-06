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
    public abstract class AutofacFilterBaseFixture<TFilter1, TFilter2, TFilterType>
        where TFilter1 : new()
        where TFilter2 : new()
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
            _baseControllerContext = new ControllerContext { Controller = new TestController() };
            _derivedControllerContext = new ControllerContext { Controller = new TestControllerA() };
            _mostDerivedControllerContext = new ControllerContext { Controller = new TestControllerB() };

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
        public void ResolvesControllerScopedFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter(
                FilterScope.Controller,
                _reflectedActionDescriptor,
                ConfigureFirstControllerRegistration());
        }

        [Test]
        public void ResolvesActionScopedFilterForReflectedActionDescriptor()
        {
            AssertSingleFilter(
                FilterScope.Action,
                _reflectedActionDescriptor,
                ConfigureFirstActionRegistration());
        }

        [Test]
        public void ResolvesActionScopedFilterForReflectedAsyncActionDescriptor()
        {
            AssertSingleFilter(
                FilterScope.Action,
                _reflectedAsyncActionDescriptor,
                ConfigureFirstActionRegistration());
        }

        [Test]
        public void ResolvesActionScopedFilterForTaskAsyncActionDescriptor()
        {
            AssertSingleFilter(
                FilterScope.Action,
                _taskAsyncActionDescriptor,
                ConfigureFirstActionRegistration());
        }

        [Test]
        public void ResolvesActionScopedFilterForImmediateBaseContoller()
        {
            AssertSingleFilter(
                FilterScope.Action,
                _derivedActionDescriptor,
                ConfigureFirstActionRegistration(),
                _derivedControllerContext);
        }

        [Test]
        public void ResolvesActionScopedFilterForMostBaseContoller()
        {
            AssertSingleFilter(
                FilterScope.Action,
                _mostDerivedActionDescriptor,
                ConfigureFirstActionRegistration(),
                _mostDerivedControllerContext);
        }

        [Test]
        public void ResolvesMultipleControllerScopedFilters()
        {
            AssertMultipleFilters(
                FilterScope.Controller,
                ConfigureFirstControllerRegistration(),
                ConfigureSecondControllerRegistration());
        }

        [Test]
        public void ResolvesMultipleActionScopedFilters()
        {
            AssertMultipleFilters(
                FilterScope.Action,
                ConfigureFirstActionRegistration(),
                ConfigureSecondActionRegistration());
        }

        [Test]
        public void ResolvesControllerScopedOverrideFilter()
        {
            AssertOverrideFilter(
                _reflectedActionDescriptor,
                ConfigureControllerFilterOverride());
        }

        [Test]
        public void ResolvesActionScopedOverrideFilterForReflectedActionDescriptor()
        {
            AssertOverrideFilter(
                _reflectedActionDescriptor,
                ConfigureActionFilterOverride());
        }

        [Test]
        public void ResolvesActionScopedOverrideFilterForReflectedAsyncActionDescriptor()
        {
            AssertOverrideFilter(
                _reflectedAsyncActionDescriptor,
                ConfigureActionFilterOverride());
        }

        [Test]
        public void ResolvesActionScopedOverrideFilterForTaskAsyncActionDescriptor()
        {
            AssertOverrideFilter(
                _taskAsyncActionDescriptor,
                ConfigureActionFilterOverride());
        }

        [Test]
        public void ResolvesActionScopedOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter(
                _reflectedActionDescriptor,
                ConfigureActionFilterOverride(),
                _derivedControllerContext);
        }

        [Test]
        public void ResolvesActionScopedOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter(
                _reflectedActionDescriptor,
                ConfigureActionFilterOverride(),
                _mostDerivedControllerContext);
        }

        [Test]
        public void ResolvesControllerScopedOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter(
                _reflectedActionDescriptor,
                ConfigureControllerFilterOverride(),
                _derivedControllerContext);
        }

        [Test]
        public void ResolvesControllerScopedOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter(
                _reflectedActionDescriptor,
                ConfigureControllerFilterOverride(),
                _mostDerivedControllerContext);
        }

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration();

        protected abstract Action<ContainerBuilder> ConfigureControllerFilterOverride();

        protected abstract Action<ContainerBuilder> ConfigureActionFilterOverride();

        static void SetupMockLifetimeScopeProvider(ILifetimeScope container)
        {
            var resolver = new AutofacDependencyResolver(container, new StubLifetimeScopeProvider(container));
            DependencyResolver.SetResolver(resolver);
        }

        void AssertSingleFilter(FilterScope filterScope, ActionDescriptor actionDescriptor,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure)
        {
            AssertSingleFilter(filterScope, actionDescriptor, configure, _baseControllerContext);
        }

        static void AssertSingleFilter(FilterScope filterScope, ActionDescriptor actionDescriptor,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure,
            ControllerContext controllerContext)
        {
            var builder = new ContainerBuilder();
            configure(builder.Register(c => new TFilter1()));
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(controllerContext, actionDescriptor).ToList();

            Assert.That(filters, Has.Count.EqualTo(1));
            Assert.That(filters[0].Instance, Is.InstanceOf<TFilter1>());
            Assert.That(filters[0].Scope, Is.EqualTo(filterScope));
        }

        void AssertMultipleFilters(FilterScope filterScope,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure1,
            Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> configure2)
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

        void AssertOverrideFilter(ActionDescriptor actionDescriptor, Action<ContainerBuilder> registration)
        {
            AssertOverrideFilter(actionDescriptor, registration, _baseControllerContext);
        }

        static void AssertOverrideFilter(ActionDescriptor actionDescriptor, 
            Action<ContainerBuilder> registration, ControllerContext controllerContext)
        {
            var builder = new ContainerBuilder();
            registration(builder);
            var container = builder.Build();
            SetupMockLifetimeScopeProvider(container);
            var provider = new AutofacFilterProvider();

            var filters = provider.GetFilters(controllerContext, actionDescriptor).ToList();

            var filter = filters.Select(info => info.Instance).OfType<AutofacOverrideFilter>().Single();
            Assert.That(filter, Is.InstanceOf<AutofacOverrideFilter>());
            Assert.That(filter.FiltersToOverride, Is.EqualTo(typeof(TFilterType)));
        }
    }
}