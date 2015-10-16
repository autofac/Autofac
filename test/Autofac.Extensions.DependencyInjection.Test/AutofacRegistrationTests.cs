using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Microsoft.Extensions.DependencyInjection;
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
            builder.Populate(new ServiceDescriptor[] {descriptor});
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
            builder.Populate(new ServiceDescriptor[] {descriptor});
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
            builder.Populate(new ServiceDescriptor[] {descriptor});
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
            builder.Populate(new ServiceDescriptor[] {descriptor});
            var container = builder.Build();

            container.AssertRegistered<Func<IServiceProvider, IService>>();
        }

        [Fact]
        public void CanGenerateFactoryService()
        {
            var builder = new ContainerBuilder();
            var descriptor = new ServiceDescriptor(typeof(IService), typeof(Service), ServiceLifetime.Transient);
            builder.Populate(new ServiceDescriptor[] {descriptor});
            var container = builder.Build();

            container.AssertRegistered<Func<IService>>();
        }

        public class Service : IService { }

        public interface IService { }
    }
}
