using System;
using Autofac;
using Autofac.Builder;
using Autofac.Extras.Multitenant;
using Autofac.Extras.Tests.Multitenant.Stubs;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant
{
    [TestFixture]
    public class RegistrationExtensionsFixture
    {
        [Test(Description = "Attempts to attach an instance-per-tenant lifetime to a null registration.")]
        public void InstancePerTenant_NullRegistration()
        {
            IRegistrationBuilder<StubDependency1Impl1, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration = null;
            Assert.Throws<ArgumentNullException>(() => registration.InstancePerTenant());
        }

        [Test(Description = "Resolves tenant-scoped implementations registered on root. Verifies lifetime is respected.")]
        public void InstancePerTenant_RespectsLifetimeScope()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var builder = new ContainerBuilder();
            builder.RegisterType<StubDependency1Impl1>().As<IStubDependency1>().InstancePerTenant();
            var mtc = new MultitenantContainer(strategy, builder.Build());

            // Two resolutions for a single tenant
            var dep1 = mtc.Resolve<IStubDependency1>();
            var dep2 = mtc.Resolve<IStubDependency1>();

            // One resolution for a different tenant
            strategy.TenantId = "tenant2";
            var dep3 = mtc.Resolve<IStubDependency1>();

            Assert.AreSame(dep1, dep2, "The two dependencies resolved for the first tenant should be the same.");
            Assert.AreNotSame(dep1, dep3, "The dependencies resolved across tenants should not be the same.");
        }

        [Test(Description = "Resolves tenant-scoped implementation registered on root with dependency registered on tenant. Verifies lifetime is respected.")]
        public void InstancePerTenant_RootAndPerTenantDependencies()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var builder = new ContainerBuilder();
            builder.RegisterType<StubDependency3Impl>().As<IStubDependency3>().InstancePerTenant();
            var mtc = new MultitenantContainer(strategy, builder.Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency1Impl1>().As<IStubDependency1>().InstancePerTenant());
            mtc.ConfigureTenant("tenant2", b => b.RegisterType<StubDependency1Impl1>().As<IStubDependency1>().InstancePerTenant());

            // Two resolutions for a single tenant
            var dep1 = mtc.Resolve<IStubDependency3>();
            var dep2 = mtc.Resolve<IStubDependency3>();

            // One resolution for a different tenant
            strategy.TenantId = "tenant2";
            var dep3 = mtc.Resolve<IStubDependency3>();

            Assert.AreSame(dep1, dep2, "The two dependencies resolved for the first tenant should be the same.");
            Assert.AreNotSame(dep1, dep3, "The dependencies resolved across tenants should not be the same.");
            Assert.AreSame(dep1.Dependency, dep2.Dependency, "The two sub-dependencies resolved for the first tenant should be the same.");
            Assert.AreNotSame(dep1.Dependency, dep3.Dependency, "The sub-dependencies resolved across tenants should not be the same.");
        }
    }
}
