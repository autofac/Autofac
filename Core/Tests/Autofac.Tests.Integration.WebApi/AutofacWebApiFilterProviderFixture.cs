using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
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
    }
}