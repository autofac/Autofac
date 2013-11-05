using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using Autofac.Builder;
using Autofac.Integration.WebApi;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public abstract class AutofacFilterBaseFixture<TFilter1, TFilter2, TFilterType>
    {
        [Test]
        public void ResolvesControllerScopedFilter()
        {
            AssertSingleFilter<TestController>(
                GetFirstRegistration(),
                ConfigureFirstControllerRegistration());
        }

        [Test]
        public void ResolvesActionScopedFilter()
        {
            AssertSingleFilter<TestController>(
                GetFirstRegistration(),
                ConfigureFirstActionRegistration());
        }

        [Test]
        public void ResolvesActionScopedFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestControllerA>(
                GetFirstRegistration(),
                ConfigureFirstActionRegistration());
        }

        [Test]
        public void ResolvesActionScopedFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestControllerB>(
                GetFirstRegistration(),
                ConfigureFirstActionRegistration());
        }

        [Test]
        public void ResolvesMultipleControllerScopedFilters()
        {
            AssertMultipleFilters(
                GetFirstRegistration(),
                GetSecondRegistration(),
                ConfigureFirstControllerRegistration(),
                ConfigureSecondControllerRegistration());
        }

        [Test]
        public void ResolvesMultipleActionScopedFilters()
        {
            AssertMultipleFilters(
                GetFirstRegistration(),
                GetSecondRegistration(),
                ConfigureFirstActionRegistration(),
                ConfigureSecondActionRegistration());
        }

        [Test]
        public void ResolvesControllerScopedOverrideFilter()
        {
            AssertOverrideFilter<TestController>(
                ConfigureControllerFilterOverride());
        }

        [Test]
        public void ResolvesActionScopedOverrideFilter()
        {
            AssertOverrideFilter<TestController>(
                ConfigureActionFilterOverride());
        }

        [Test]
        public void ResolvesActionScopedOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<TestControllerA>(
                ConfigureActionFilterOverride());
        }

        [Test]
        public void ResolvesActionScopedOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<TestControllerB>(
                ConfigureActionFilterOverride());
        }

        [Test]
        public void ResolvesControllerScopedOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<TestControllerA>(
                ConfigureControllerFilterOverride());
        }

        [Test]
        public void ResolvesControllerScopedOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<TestControllerB>(
                ConfigureControllerFilterOverride());
        }

        protected abstract Func<IComponentContext, TFilter1> GetFirstRegistration();

        protected abstract Func<IComponentContext, TFilter2> GetSecondRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration();

        protected abstract Type GetWrapperType();

        protected abstract Action<ContainerBuilder> ConfigureControllerFilterOverride();

        protected abstract Action<ContainerBuilder> ConfigureActionFilterOverride();

        static ReflectedHttpActionDescriptor BuildActionDescriptorForGetMethod()
        {
            return BuildActionDescriptorForGetMethod(typeof(TestController));
        }

        static ReflectedHttpActionDescriptor BuildActionDescriptorForGetMethod(Type controllerType)
        {
            var controllerDescriptor = new HttpControllerDescriptor { ControllerType = controllerType };
            var methodInfo = typeof(TestController).GetMethod("Get");
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            return actionDescriptor;
        }

        void AssertSingleFilter<TController>(
            Func<IComponentContext, TFilter1> registration,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure)
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            configure(builder.Register(registration).InstancePerApiRequest());
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController));

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var wrapperType = GetWrapperType();
            var filter = filterInfos.Select(info => info.Instance).Single(i => i.GetType() == wrapperType);
            Assert.That(filter, Is.InstanceOf(wrapperType));
        }

        void AssertMultipleFilters(
            Func<IComponentContext, TFilter1> registration1,
            Func<IComponentContext, TFilter2> registration2,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure1,
            Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> configure2)
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            configure1(builder.Register(registration1).InstancePerApiRequest());
            configure2(builder.Register(registration2).InstancePerApiRequest());
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var wrapperType = GetWrapperType();
            var filters = filterInfos.Select(info => info.Instance).Where(i => i.GetType() == wrapperType).ToArray();
            Assert.That(filters, Has.Length.EqualTo(1));
            Assert.That(filters[0], Is.InstanceOf(wrapperType));
        }

        static void AssertOverrideFilter<TController>(Action<ContainerBuilder> registration)
        {
            var builder = new ContainerBuilder();
            registration(builder);
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController));

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var filter = filterInfos.Select(info => info.Instance).OfType<AutofacOverrideFilter>().Single();
            Assert.That(filter, Is.InstanceOf<AutofacOverrideFilter>());
            Assert.That(filter.AllowMultiple, Is.False);
            Assert.That(filter.FiltersToOverride, Is.EqualTo(typeof(TFilterType)));
        }
    }
}
