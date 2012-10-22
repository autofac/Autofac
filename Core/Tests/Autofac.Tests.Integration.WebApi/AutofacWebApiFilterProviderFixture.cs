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
    public class AutofacWebApiFilterProviderFixture
    {
        [Test]
        public void FilterRegistrationsWithoutMetadataIgnored()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AuthorizeAttribute>().AsImplementedInterfaces();
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor);

            Assert.That(filterInfos.Select(f => f.Instance).OfType<AuthorizeAttribute>().Any(), Is.False);
        }

        [Test]
        public void InjectsFilterPropertiesForRegisteredDependencies()
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var filter = filterInfos.Select(info => info.Instance).OfType<CustomActionFilter>().Single();
            Assert.That(filter.Logger, Is.InstanceOf<ILogger>());
        }

        [Test]
        public void ReturnsFiltersWithoutPropertyInjectionForUnregisteredDependencies()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var filter = filterInfos.Select(info => info.Instance).OfType<CustomActionFilter>().Single();
            Assert.That(filter.Logger, Is.Null);
        }

        [Test]
        public void AsActionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestActionFilter(c.Resolve<ILogger>())).AsActionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedActionFilter()
        {
            AssertSingleFilter<TestActionFilter, ActionFilterWrapper, TestController>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                r => r.AsActionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedActionFilter()
        {
            AssertSingleFilter<TestActionFilter, ActionFilterWrapper, TestController>(
                c => new TestActionFilter(c.Resolve<ILogger>()), 
                r => r.AsActionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedActionFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestActionFilter, ActionFilterWrapper, TestControllerA>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                r => r.AsActionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedActionFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestActionFilter, ActionFilterWrapper, TestControllerB>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                r => r.AsActionFilterFor<TestController>(t => t.Get()));
        }

        [Test]
        public void ResolvesMultipleControllerScopedActionFilters()
        {
            AssertMultipleFilters<TestActionFilter, TestActionFilter2, ActionFilterWrapper>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                c => new TestActionFilter2(c.Resolve<ILogger>()),
                r => r.AsActionFilterFor<TestController>(),
                r => r.AsActionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesMultipleActionScopedActionFilters()
        {
            AssertMultipleFilters<TestActionFilter, TestActionFilter2, ActionFilterWrapper>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                c => new TestActionFilter2(c.Resolve<ILogger>()),
                r => r.AsActionFilterFor<TestController>(c => c.Get()),
                r => r.AsActionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void AsAuthorizationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestAuthorizationFilter(c.Resolve<ILogger>())).AsAuthorizationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedAuthorizationFilter()
        {
            AssertSingleFilter<TestAuthorizationFilter, AuthorizationFilterWrapper, TestController>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                r => r.AsAuthorizationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilter()
        {
            AssertSingleFilter<TestAuthorizationFilter, AuthorizationFilterWrapper, TestController>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestAuthorizationFilter, AuthorizationFilterWrapper, TestControllerA>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestAuthorizationFilter, AuthorizationFilterWrapper, TestControllerB>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                r => r.AsAuthorizationFilterFor<TestController>(t => t.Get()));
        }

        [Test]
        public void ResolvesMultipleControllerScopedAuthorizationFilters()
        {
            AssertMultipleFilters<TestAuthorizationFilter, TestAuthorizationFilter2, AuthorizationFilterWrapper>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                c => new TestAuthorizationFilter2(c.Resolve<ILogger>()),
                r => r.AsAuthorizationFilterFor<TestController>(),
                r => r.AsAuthorizationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesMultipleActionScopedAuthorizationFilters()
        {
            AssertMultipleFilters<TestAuthorizationFilter, TestAuthorizationFilter2, AuthorizationFilterWrapper>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                c => new TestAuthorizationFilter2(c.Resolve<ILogger>()),
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Get()),
                r => r.AsAuthorizationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void AsExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestExceptionFilter(c.Resolve<ILogger>())).AsExceptionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedExceptionFilter()
        {
            AssertSingleFilter<TestExceptionFilter, ExceptionFilterWrapper, TestController>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                r => r.AsExceptionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedExceptionFilter()
        {
            AssertSingleFilter<TestExceptionFilter, ExceptionFilterWrapper, TestController>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                r => r.AsExceptionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestExceptionFilter, ExceptionFilterWrapper, TestControllerA>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                r => r.AsExceptionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestExceptionFilter, ExceptionFilterWrapper, TestControllerB>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                r => r.AsExceptionFilterFor<TestController>(t => t.Get()));
        }

        [Test]
        public void ResolvesMultipleControllerScopedExceptionFilters()
        {
            AssertMultipleFilters<TestExceptionFilter, TestExceptionFilter2, ExceptionFilterWrapper>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                c => new TestExceptionFilter2(c.Resolve<ILogger>()),
                r => r.AsExceptionFilterFor<TestController>(),
                r => r.AsExceptionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesMultipleActionScopedExceptionFilters()
        {
            AssertMultipleFilters<TestExceptionFilter, TestExceptionFilter2, ExceptionFilterWrapper>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                c => new TestExceptionFilter2(c.Resolve<ILogger>()),
                r => r.AsExceptionFilterFor<TestController>(c => c.Get()),
                r => r.AsExceptionFilterFor<TestController>(c => c.Get()));
        }

        static ReflectedHttpActionDescriptor BuildActionDescriptorForGetMethod()
        {
            return BuildActionDescriptorForGetMethod(typeof(TestController));
        }

        static ReflectedHttpActionDescriptor BuildActionDescriptorForGetMethod(Type controllerType)
        {
            var controllerDescriptor = new HttpControllerDescriptor {ControllerType = controllerType};
            var methodInfo = typeof(TestController).GetMethod("Get");
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            return actionDescriptor;
        }

        static void AssertSingleFilter<TFilter, TWrapper, TController>(
            Func<IComponentContext, TFilter> registration,
            Action<IRegistrationBuilder<TFilter, SimpleActivatorData, SingleRegistrationStyle>> configure)
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            configure(builder.Register(registration).InstancePerApiRequest());
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var actionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController));

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var filter = filterInfos.Select(info => info.Instance).OfType<TWrapper>().Single();
            Assert.That(filter, Is.InstanceOf<TWrapper>());
        }

        static void AssertMultipleFilters<TFilter1, TFilter2, TWrapper>(
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
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var filters = filterInfos.Select(info => info.Instance).OfType<TWrapper>().ToArray();
            Assert.That(filters, Has.Length.EqualTo(2));
            Assert.That(filters[0], Is.InstanceOf<TWrapper>());
            Assert.That(filters[1], Is.InstanceOf<TWrapper>());
        }
    }
}