using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Tests.Scenarios.ScannedAssembly;
using Autofac.Core;
using Autofac.Core.Lifetime;

namespace Autofac.Tests
{
    [TestFixture]
    public class ScanningRegistrationTests
    {
        [Test]
        public void WhenAssemblyIsScanned_TypesRegisteredByDefault()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly);
            var c = cb.Build();

            c.AssertRegistered<AComponent>();
            c.AssertSharing<AComponent>(InstanceSharing.None);
            c.AssertLifetime<AComponent, CurrentScopeLifetime>();
            c.AssertOwnership<AComponent>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Test]
        public void WhenFiltersApplied_NonMatchingTypesExcluded()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .Where(t => t.Name.StartsWith("A"))
                .Where(t => t.Name.StartsWith("AC"));
            var c = cb.Build();

            c.AssertRegistered<AComponent>();
            c.AssertNotRegistered<BComponent>();
            c.AssertNotRegistered<A2Component>();
        }

        [Test]
        public void WhenServiceSpecifiedDirectly_AllMatchingTypesImplementIt()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .As(typeof(object));
            var c = cb.Build();

            c.AssertRegistered<object>();
            c.AssertNotRegistered<AComponent>();
        }

        [Test]
        public void WhenServicesSpecifiedByFunction_EachTypeMappedIndependently()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .As(t => new NamedService(t.Name));
            var c = cb.Build();

            c.AssertRegistered("AComponent");
            c.AssertRegistered("BComponent");
            c.AssertNotRegistered<AComponent>();
        }

        [Test]
        public void NameMappingRegistersNamedServices()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .Named(t => t.Name);
            var c = cb.Build();

            c.AssertRegistered("AComponent");
        }

        [Test]
        public void TypeMappingRegistersTypedServices()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .As(t => typeof(object));
            var c = cb.Build();

            c.AssertRegistered<object>();
        }

        [Test]
        public void AsImplementedInterfaces_RegistersImplementedInterfaces()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .AsImplementedInterfaces();
            var c = cb.Build();

            c.AssertRegistered<IAService>();
            c.AssertRegistered<IBService>();
        }

        [Test]
        public void WhenFilterApplied_DefaultSelfRegistrationOmitted()
        {
            var cb = new ContainerBuilder();
            cb.RegisterAssemblyTypes(typeof(AComponent).Assembly)
                .AsImplementedInterfaces();
            var c = cb.Build();

            c.AssertNotRegistered<AComponent>();
        }
    }
}
