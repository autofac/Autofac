using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public class AutofacRegistrationTests
    {
        [Fact]
        public void PopulateRegistersServiceProvider()
        {
            var builder = new ContainerBuilder();
            builder.Populate(Enumerable.Empty<ServiceDescriptor>());
            var container = builder.Build();

            container.AssertRegistered<IServiceProvider>();
        }

        [Fact]
        public void CorrectServiceProviderIsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Populate(Enumerable.Empty<ServiceDescriptor>());
            var container = builder.Build();

            container.AssertImplementation<IServiceProvider, AutofacServiceProvider>();
        }

        [Fact]
        public void PopulateRegistersServiceScopeFactory()
        {
            var builder = new ContainerBuilder();
            builder.Populate(Enumerable.Empty<ServiceDescriptor>());
            var container = builder.Build();

            container.AssertRegistered<IServiceScopeFactory>();
        }

        [Fact]
        public void ServiceScopeFactoryIsRegistered()
        {
            var builder = new ContainerBuilder();
            builder.Populate(Enumerable.Empty<ServiceDescriptor>());
            var container = builder.Build();

            container.AssertImplementation<IServiceScopeFactory, AutofacServiceScopeFactory>();
        }

        [Fact]
        public void CanRegisterTransientService()
        {
            var builder = new ContainerBuilder();
            var descriptor = new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Transient);
            builder.Populate(new ServiceDescriptor[] { descriptor });
            var container = builder.Build();

            container.AssertLifetime<IService, CurrentScopeLifetime>();
            container.AssertSharing<IService>(InstanceSharing.None);
            container.AssertOwnership<IService>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void CanRegisterSingletonService()
        {
            var builder = new ContainerBuilder();
            var descriptor = new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Singleton);
            builder.Populate(new ServiceDescriptor[] { descriptor });
            var container = builder.Build();

            container.AssertLifetime<IService, RootScopeLifetime>();
            container.AssertSharing<IService>(InstanceSharing.Shared);
            container.AssertOwnership<IService>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void CanRegisterScopedService()
        {
            var builder = new ContainerBuilder();
            var descriptor = new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Scoped);
            builder.Populate(new ServiceDescriptor[] { descriptor });
            var container = builder.Build();

            container.AssertLifetime<IService, CurrentScopeLifetime>();
            container.AssertSharing<IService>(InstanceSharing.Shared);
            container.AssertOwnership<IService>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void CanRegisterGenericService()
        {
            var builder = new ContainerBuilder();
            var descriptor = new ServiceDescriptor(typeof(IList<>), typeof(List<>), ServiceLifetime.Scoped);
            builder.Populate(new ServiceDescriptor[] { descriptor });
            var container = builder.Build();

            container.AssertRegistered<IList<IService>>();
        }

        [Fact]
        public void CanRegisterFactoryService()
        {
            var builder = new ContainerBuilder();
            var descriptor = new ServiceDescriptor(typeof(IService), sp => new Service(), ServiceLifetime.Transient);
            builder.Populate(new ServiceDescriptor[] { descriptor });
            var container = builder.Build();

            container.AssertRegistered<Func<IServiceProvider, IService>>();
        }

        [Fact]
        public void CanGenerateFactoryService()
        {
            var builder = new ContainerBuilder();
            var descriptor = new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Transient);
            builder.Populate(new ServiceDescriptor[] { descriptor });
            var container = builder.Build();

            container.AssertRegistered<Func<IService>>();
        }

        [Fact]
        public void ServiceCollectionConfigurationIsRetainedInRootContainer()
        {
            var collection = new ServiceCollection();
            collection.AddOptions();
            collection.Configure<TestOptions>(options =>
            {
                options.Value = 5;
            });

            var builder = new ContainerBuilder();
            builder.Populate(collection);
            var container = builder.Build();

            var resolved = container.Resolve<IOptions<TestOptions>>();
            Assert.NotNull(resolved.Value);
            Assert.Equal(5, resolved.Value.Value);
        }

        [Fact]
        public void RegistrationsAddedAfterPopulateComeLastWhenResolvedWithIEnumerable()
        {
            const string s1 = "s1";
            const string s2 = "s2";
            const string s3 = "s3";
            const string s4 = "s4";

            var collection = new ServiceCollection();
            collection.AddTransient(provider => s1);
            collection.AddTransient(provider => s2);
            var builder = new ContainerBuilder();
            builder.Populate(collection);
            builder.Register(c => s3);
            builder.Register(c => s4);
            var container = builder.Build();

            var resolved = container.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(resolved, new[] { s1, s2, s3, s4 });
        }

        [Fact]
        public void RegistrationsAddedBeforePopulateComeFirstWhenResolvedWithIEnumerable()
        {
            const string s1 = "s1";
            const string s2 = "s2";
            const string s3 = "s3";
            const string s4 = "s4";

            var builder = new ContainerBuilder();
            builder.Register(c => s1);
            builder.Register(c => s2);
            var collection = new ServiceCollection();
            collection.AddTransient(provider => s3);
            collection.AddTransient(provider => s4);
            builder.Populate(collection);
            var container = builder.Build();

            var resolved = container.Resolve<IEnumerable<string>>().ToArray();

            Assert.Equal(resolved, new[] { s1, s2, s3, s4 });
        }

        public class Service : IService
        {
        }

        public interface IService
        {
        }

        public class TestOptions
        {
            public int Value { get; set; }
        }
    }
}
