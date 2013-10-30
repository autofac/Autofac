using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
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
        public void CanRegisterMultipleFilterTypesAgainstSingleService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new TestCombinationFilter())
                .AsWebApiActionFilterFor<TestController>()
                .AsWebApiAuthenticationFilterFor<TestController>()
                .AsWebApiAuthorizationFilterFor<TestController>()
                .AsWebApiExceptionFilterFor<TestController>();
            var container = builder.Build();

            Assert.That(container.Resolve<IAutofacActionFilter>(), Is.Not.Null);
            Assert.That(container.Resolve<IAutofacAuthenticationFilter>(), Is.Not.Null);
            Assert.That(container.Resolve<IAutofacAuthorizationFilter>(), Is.Not.Null);
            Assert.That(container.Resolve<IAutofacExceptionFilter>(), Is.Not.Null);
        }

        [Test]
        public void ResolvesMultipleFiltersOfDifferentTypes()
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();

            builder.Register(c => new TestAuthenticationFilter(c.Resolve<ILogger>()))
                .AsWebApiAuthenticationFilterFor<TestController>()
                .InstancePerApiRequest();

            builder.Register(c => new TestAuthorizationFilter(c.Resolve<ILogger>()))
                .AsWebApiAuthorizationFilterFor<TestController>()
                .InstancePerApiRequest();

            builder.Register(c => new TestExceptionFilter(c.Resolve<ILogger>()))
                .AsWebApiExceptionFilterFor<TestController>()
                .InstancePerApiRequest();

            builder.Register(c => new TestActionFilter(c.Resolve<ILogger>()))
                .AsWebApiActionFilterFor<TestController>()
                .InstancePerApiRequest();

            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration
            {
                DependencyResolver = new AutofacWebApiDependencyResolver(container)
            };
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();
            var filters = filterInfos.Select(info => info.Instance).ToArray();

            Assert.That(filters.OfType<AuthenticationFilterWrapper>().Count(), Is.EqualTo(1));
            Assert.That(filters.OfType<AuthorizationFilterWrapper>().Count(), Is.EqualTo(1));
            Assert.That(filters.OfType<ExceptionFilterWrapper>().Count(), Is.EqualTo(1));
            Assert.That(filters.OfType<ActionFilterWrapper>().Count(), Is.EqualTo(1));
        }

        // Action filters

        [Test]
        public void AsActionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestActionFilter(c.Resolve<ILogger>())).AsWebApiActionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ServiceTypeMustBeActionFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiActionFilterFor<TestController>());

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void ResolvesControllerScopedActionFilter()
        {
            AssertSingleFilter<TestActionFilter, ActionFilterWrapper, TestController>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiActionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedActionFilter()
        {
            AssertSingleFilter<TestActionFilter, ActionFilterWrapper, TestController>(
                c => new TestActionFilter(c.Resolve<ILogger>()), 
                r => r.AsWebApiActionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedActionFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestActionFilter, ActionFilterWrapper, TestControllerA>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiActionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedActionFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestActionFilter, ActionFilterWrapper, TestControllerB>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiActionFilterFor<TestController>(t => t.Get()));
        }

        [Test]
        public void ResolvesMultipleControllerScopedActionFilters()
        {
            AssertMultipleFilters<TestActionFilter, TestActionFilter2, ActionFilterWrapper>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                c => new TestActionFilter2(c.Resolve<ILogger>()),
                r => r.AsWebApiActionFilterFor<TestController>(),
                r => r.AsWebApiActionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesMultipleActionScopedActionFilters()
        {
            AssertMultipleFilters<TestActionFilter, TestActionFilter2, ActionFilterWrapper>(
                c => new TestActionFilter(c.Resolve<ILogger>()),
                c => new TestActionFilter2(c.Resolve<ILogger>()),
                r => r.AsWebApiActionFilterFor<TestController>(c => c.Get()),
                r => r.AsWebApiActionFilterFor<TestController>(c => c.Get()));
        }

        // Authorization filter

        [Test]
        public void AsAuthorizationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestAuthorizationFilter(c.Resolve<ILogger>())).AsWebApiAuthorizationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ServiceTypeMustAuthorizationFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthorizationFilterFor<TestController>());

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void ResolvesControllerScopedAuthorizationFilter()
        {
            AssertSingleFilter<TestAuthorizationFilter, AuthorizationFilterWrapper, TestController>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthorizationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilter()
        {
            AssertSingleFilter<TestAuthorizationFilter, AuthorizationFilterWrapper, TestController>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthorizationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestAuthorizationFilter, AuthorizationFilterWrapper, TestControllerA>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthorizationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestAuthorizationFilter, AuthorizationFilterWrapper, TestControllerB>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthorizationFilterFor<TestController>(t => t.Get()));
        }

        [Test]
        public void ResolvesMultipleControllerScopedAuthorizationFilters()
        {
            AssertMultipleFilters<TestAuthorizationFilter, TestAuthorizationFilter2, AuthorizationFilterWrapper>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                c => new TestAuthorizationFilter2(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthorizationFilterFor<TestController>(),
                r => r.AsWebApiAuthorizationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesMultipleActionScopedAuthorizationFilters()
        {
            AssertMultipleFilters<TestAuthorizationFilter, TestAuthorizationFilter2, AuthorizationFilterWrapper>(
                c => new TestAuthorizationFilter(c.Resolve<ILogger>()),
                c => new TestAuthorizationFilter2(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthorizationFilterFor<TestController>(c => c.Get()),
                r => r.AsWebApiAuthorizationFilterFor<TestController>(c => c.Get()));
        }

        // Exception filters

        [Test]
        public void AsExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestExceptionFilter(c.Resolve<ILogger>())).AsWebApiExceptionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ServiceTypeMustExceptionFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthorizationFilterFor<TestController>());

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void ResolvesControllerScopedExceptionFilter()
        {
            AssertSingleFilter<TestExceptionFilter, ExceptionFilterWrapper, TestController>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiExceptionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedExceptionFilter()
        {
            AssertSingleFilter<TestExceptionFilter, ExceptionFilterWrapper, TestController>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiExceptionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestExceptionFilter, ExceptionFilterWrapper, TestControllerA>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiExceptionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedExceptionFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestExceptionFilter, ExceptionFilterWrapper, TestControllerB>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiExceptionFilterFor<TestController>(t => t.Get()));
        }

        [Test]
        public void ResolvesMultipleControllerScopedExceptionFilters()
        {
            AssertMultipleFilters<TestExceptionFilter, TestExceptionFilter2, ExceptionFilterWrapper>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                c => new TestExceptionFilter2(c.Resolve<ILogger>()),
                r => r.AsWebApiExceptionFilterFor<TestController>(),
                r => r.AsWebApiExceptionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesMultipleActionScopedExceptionFilters()
        {
            AssertMultipleFilters<TestExceptionFilter, TestExceptionFilter2, ExceptionFilterWrapper>(
                c => new TestExceptionFilter(c.Resolve<ILogger>()),
                c => new TestExceptionFilter2(c.Resolve<ILogger>()),
                r => r.AsWebApiExceptionFilterFor<TestController>(c => c.Get()),
                r => r.AsWebApiExceptionFilterFor<TestController>(c => c.Get()));
        }

        // Authentication filters

        [Test]
        public void AsAuthenticationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestAuthenticationFilter(c.Resolve<ILogger>())).AsWebApiAuthenticationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ServiceTypeMustBeAuthenticationFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthenticationFilterFor<TestController>());

            Assert.That(exception.ParamName, Is.EqualTo("registration"));
        }

        [Test]
        public void ResolvesControllerScopedAuthenticationFilter()
        {
            AssertSingleFilter<TestAuthenticationFilter, AuthenticationFilterWrapper, TestController>(
                c => new TestAuthenticationFilter(c.Resolve<ILogger>()), 
                r => r.AsWebApiAuthenticationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedAuthenticationFilter()
        {
            AssertSingleFilter<TestAuthenticationFilter, AuthenticationFilterWrapper, TestController>(
                c => new TestAuthenticationFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthenticationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthenticationFilterForImmediateBaseContoller()
        {
            AssertSingleFilter<TestAuthenticationFilter, AuthenticationFilterWrapper, TestControllerA>(
                c => new TestAuthenticationFilter(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthenticationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthenticationFilterForMostBaseContoller()
        {
            AssertSingleFilter<TestAuthenticationFilter, AuthenticationFilterWrapper, TestControllerB>(
                c => new TestAuthenticationFilter(c.Resolve<ILogger>()), 
                r => r.AsWebApiAuthenticationFilterFor<TestController>(t => t.Get()));
        }

        [Test]
        public void ResolvesMultipleControllerScopedAuthenticationFilters()
        {
            AssertMultipleFilters<TestAuthenticationFilter, TestAuthenticationFilter2, AuthenticationFilterWrapper>(
                c => new TestAuthenticationFilter(c.Resolve<ILogger>()),
                c => new TestAuthenticationFilter2(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthenticationFilterFor<TestController>(),
                r => r.AsWebApiAuthenticationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesMultipleActionScopedAuthenticationFilters()
        {
            AssertMultipleFilters<TestAuthenticationFilter, TestAuthenticationFilter2, AuthenticationFilterWrapper>(
                c => new TestAuthenticationFilter(c.Resolve<ILogger>()),
                c => new TestAuthenticationFilter2(c.Resolve<ILogger>()),
                r => r.AsWebApiAuthenticationFilterFor<TestController>(c => c.Get()),
                r => r.AsWebApiAuthenticationFilterFor<TestController>(c => c.Get()));
        }

        // Action filter override

        [Test]
        public void OverrideActionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideActionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedOverrideActionFilter()
        {
            AssertOverrideFilter<IActionFilter, TestController>(
                builder => builder.OverrideActionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedOverrideActionFilter()
        {
            AssertOverrideFilter<IActionFilter, TestController>(
                builder => builder.OverrideActionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedActionOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<IActionFilter, TestControllerA>(
                builder => builder.OverrideActionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedActionOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<IActionFilter, TestControllerB>(
                builder => builder.OverrideActionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesControllerScopedActionOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<IActionFilter, TestControllerA>(
                builder => builder.OverrideActionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesControllerScopedActionOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<IActionFilter, TestControllerB>(
                builder => builder.OverrideActionFilterFor<TestController>());
        }

        // Authorization filter override

        [Test]
        public void OverrideAuthorizationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideAuthorizationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedOverrideAuthorizationFilter()
        {
            AssertOverrideFilter<IAuthorizationFilter, TestController>(
                builder => builder.OverrideAuthorizationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedOverrideAuthorizationFilter()
        {
            AssertOverrideFilter<IAuthorizationFilter, TestController>(
                builder => builder.OverrideAuthorizationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<IAuthorizationFilter, TestControllerA>(
                builder => builder.OverrideAuthorizationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthorizationOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<IAuthorizationFilter, TestControllerB>(
                builder => builder.OverrideAuthorizationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesControllerScopedAuthorizationOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<IAuthorizationFilter, TestControllerA>(
                builder => builder.OverrideAuthorizationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesControllerScopedAuthorizationOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<IAuthorizationFilter, TestControllerB>(
                builder => builder.OverrideAuthorizationFilterFor<TestController>());
        }

        // Exception filter override

        [Test]
        public void OverrideExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideExceptionFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedOverrideExceptionFilter()
        {
            AssertOverrideFilter<IExceptionFilter, TestController>(
                builder => builder.OverrideExceptionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedOverrideExceptionFilter()
        {
            AssertOverrideFilter<IExceptionFilter, TestController>(
                builder => builder.OverrideExceptionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedExceptionOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<IExceptionFilter, TestControllerA>(
                builder => builder.OverrideExceptionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedExceptionOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<IExceptionFilter, TestControllerB>(
                builder => builder.OverrideExceptionFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesControllerScopedExceptionOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<IExceptionFilter, TestControllerA>(
                builder => builder.OverrideExceptionFilterFor<TestController>());
        }

        [Test]
        public void ResolvesControllerScopedExceptionOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<IExceptionFilter, TestControllerB>(
                builder => builder.OverrideExceptionFilterFor<TestController>());
        }

        // Authentication filter override

        [Test]
        public void OverrideAuthenticationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideAuthenticationFilterFor<TestController>(null));
            Assert.That(exception.ParamName, Is.EqualTo("actionSelector"));
        }

        [Test]
        public void ResolvesControllerScopedOverrideAuthenticationFilter()
        {
            AssertOverrideFilter<IAuthenticationFilter, TestController>(
                builder => builder.OverrideAuthenticationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesActionScopedOverrideAuthenticationFilter()
        {
            AssertOverrideFilter<IAuthenticationFilter, TestController>(
                builder => builder.OverrideAuthenticationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthenticationOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<IAuthenticationFilter, TestControllerA>(
                builder => builder.OverrideAuthenticationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesActionScopedAuthenticationOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<IAuthenticationFilter, TestControllerB>(
                builder => builder.OverrideAuthenticationFilterFor<TestController>(c => c.Get()));
        }

        [Test]
        public void ResolvesControllerScopedAuthenticationOverrideFilterForImmediateBaseContoller()
        {
            AssertOverrideFilter<IAuthenticationFilter, TestControllerA>(
                builder => builder.OverrideAuthenticationFilterFor<TestController>());
        }

        [Test]
        public void ResolvesControllerScopedAuthenticationOverrideFilterForMostBaseContoller()
        {
            AssertOverrideFilter<IAuthenticationFilter, TestControllerB>(
                builder => builder.OverrideAuthenticationFilterFor<TestController>());
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
            Assert.That(filters, Has.Length.EqualTo(1));
            Assert.That(filters[0], Is.InstanceOf<TWrapper>());
        }

        static void AssertOverrideFilter<TFilterType, TController>(Action<ContainerBuilder> registration)
        {
            var builder = new ContainerBuilder();
            registration(builder);
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var actionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController));

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var filter = filterInfos.Select(info => info.Instance).OfType<AutofacOverrideFilter>().Single();
            Assert.That(filter, Is.InstanceOf<AutofacOverrideFilter>());
            Assert.That(filter.AllowMultiple, Is.False);
            Assert.That(filter.FiltersToOverride, Is.EqualTo(typeof(TFilterType)));
        }
    }
}