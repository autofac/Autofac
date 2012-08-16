using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public class InjectControllerServicesAttributeFixture
    {
        [Test]
        public void ExceptionThrownWhenAutofacDependencyResolverMissing()
        {
            var attribute = new InjectControllerServicesAttribute();
            var configuration = new HttpConfiguration();
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));

            Assert.Throws<InvalidOperationException>(() => attribute.Initialize(settings, descriptor));
        }

        [Test]
        public void HandlesMissingHttpConfiguration()
        {
            var attribute = new InjectControllerServicesAttribute();
            var settings = new HttpControllerSettings(new HttpConfiguration());
            var descriptor = new HttpControllerDescriptor();

            Assert.DoesNotThrow(() => attribute.Initialize(settings, descriptor));
        }

        [Test]
        public void IntializationRunOncePerControllerType()
        {
            var builder = new ContainerBuilder();
            var service = new Mock<IHttpActionSelector>().Object;
            int callCount = 0;
            builder.Register(c => service)
                .As<IHttpActionSelector>()
                .InstancePerApiControllerType(typeof(TestController))
                .OnActivated(e => callCount++);
            var container = builder.Build();
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new InjectControllerServicesAttribute();

            attribute.Initialize(settings, descriptor);
            attribute.Initialize(settings, descriptor);

            Assert.That(callCount, Is.EqualTo(1));
        }


        [Test]
        public void PerControllerServiceDoesNotOverrideDefault()
        {
            var builder = new ContainerBuilder();
            var service = new Mock<IHttpActionSelector>().Object;
            builder.Register(c => service)
                .As<IHttpActionSelector>()
                .InstancePerApiControllerType(typeof(TestController));
            var container = builder.Build();
            var resolver = new AutofacWebApiDependencyResolver(container);
            var configuration = new HttpConfiguration {DependencyResolver = resolver};
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new InjectControllerServicesAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.That(configuration.Services.GetActionSelector(), Is.Not.SameAs(service));
        }

        [Test]
        public void UsesDefaultServiceWhenNoKeyedServiceRegistered()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new InjectControllerServicesAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.That(settings.Services.GetActionSelector(), Is.InstanceOf<ApiControllerActionSelector>());
        }

        [Test]
        public void UsesRootServiceWhenNoKeyedServiceRegistered()
        {
            var builder = new ContainerBuilder();
            var service = new Mock<IHttpActionSelector>().Object;
            builder.RegisterInstance(service);
            var container = builder.Build();
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new InjectControllerServicesAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.That(settings.Services.GetActionSelector(), Is.EqualTo(service));
        }

        [Test]
        public void FormattersCanBeResolvedPerControllerType()
        {
            var builder = new ContainerBuilder();
            var formatter1 = new Mock<MediaTypeFormatter>().Object;
            var formatter2 = new Mock<MediaTypeFormatter>().Object;
            builder.RegisterInstance(formatter1).InstancePerApiControllerType(typeof(TestController));
            builder.RegisterInstance(formatter2).InstancePerApiControllerType(typeof(TestController));
            var container = builder.Build();
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new InjectControllerServicesAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.That(settings.Formatters.Contains(formatter1), Is.True);
            Assert.That(settings.Formatters.Contains(formatter2), Is.True);
        }

        [Test]
        public void ExistingFormattersCanBeCleared()
        {
            var builder = new ContainerBuilder();
            var formatter1 = new Mock<MediaTypeFormatter>().Object;
            var formatter2 = new Mock<MediaTypeFormatter>().Object;
            builder.RegisterInstance(formatter1).InstancePerApiControllerType(typeof(TestController), true);
            builder.RegisterInstance(formatter2).InstancePerApiControllerType(typeof(TestController), true);
            var container = builder.Build();
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new InjectControllerServicesAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.That(settings.Formatters.Count, Is.EqualTo(2));
            Assert.That(settings.Formatters.Contains(formatter1), Is.True);
            Assert.That(settings.Formatters.Contains(formatter2), Is.True);
        }

        [Test]
        public void ExistingListServicesCanBeCleared()
        {
            var builder = new ContainerBuilder();
            var provider1 = new Mock<ModelBinderProvider>().Object;
            var provider2 = new Mock<ModelBinderProvider>().Object;
            builder.RegisterInstance(provider1).InstancePerApiControllerType(typeof(TestController), true);
            builder.RegisterInstance(provider2).InstancePerApiControllerType(typeof(TestController), true);
            var container = builder.Build();
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new InjectControllerServicesAttribute();

            attribute.Initialize(settings, descriptor);

            var services = settings.Services.GetServices(typeof(ModelBinderProvider)).ToArray();

            Assert.That(services.Count(), Is.EqualTo(2));
            Assert.That(services.Contains(provider1), Is.True);
            Assert.That(services.Contains(provider2), Is.True);
        }

        [Test]
        public void SupportsActionInvoker()
        {
            AssertControllerServiceReplaced(services => services.GetActionInvoker());
        }

        [Test]
        public void SupportsActionSelector()
        {
            AssertControllerServiceReplaced(services => services.GetActionSelector());
        }

        [Test]
        public void SupportsActionValueBinder()
        {
            AssertControllerServiceReplaced(services => services.GetActionValueBinder());
        }

        [Test]
        public void SupportsBodyModelValidator()
        {
            AssertControllerServiceReplaced(services => services.GetBodyModelValidator());
        }

        [Test]
        public void SupportsContentNegotiator()
        {
            AssertControllerServiceReplaced(services => services.GetContentNegotiator());
        }

        [Test]
        public void SupportsHttpControllerActivator()
        {
            AssertControllerServiceReplaced(services => services.GetHttpControllerActivator());
        }

        [Test]
        public void SupportsModelMetadataProvider()
        {
            AssertControllerServiceReplaced(services => services.GetHttpControllerActivator());
        }

        [Test]
        public void SupportsModelBinderProviders()
        {
            AssertControllerServicesReplaced(services => services.GetModelBinderProviders());
        }

        [Test]
        public void SupportsModelValidatorProviders()
        {
            AssertControllerServicesReplaced(services => services.GetModelValidatorProviders());
        }

        [Test]
        public void SupportsValueProviderFactories()
        {
            AssertControllerServicesReplaced(services => services.GetValueProviderFactories());
        }

        static void AssertControllerServiceReplaced<TLimit>(Func<ServicesContainer, TLimit> serviceLocator) where TLimit : class
        {
            var builder = new ContainerBuilder();
            var service = new Mock<TLimit>().Object;
            builder.RegisterInstance(service).As<TLimit>().InstancePerApiControllerType(typeof(TestController));
            var container = builder.Build();
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new InjectControllerServicesAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.That(serviceLocator(settings.Services), Is.SameAs(service));
            Assert.That(serviceLocator(configuration.Services), Is.Not.SameAs(service));
        }

        static void AssertControllerServicesReplaced<TLimit>(Func<ServicesContainer, IEnumerable<TLimit>> serviceLocator) where TLimit : class
        {
            var builder = new ContainerBuilder();
            var service = new Mock<TLimit>().Object;
            builder.RegisterInstance(service).As<TLimit>().InstancePerApiControllerType(typeof(TestController));
            var container = builder.Build();
            var configuration = new HttpConfiguration {DependencyResolver = new AutofacWebApiDependencyResolver(container)};
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new InjectControllerServicesAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.That(serviceLocator(settings.Services).Contains(service), Is.True);
            Assert.That(serviceLocator(configuration.Services).Contains(service), Is.False);
        }
    }
}