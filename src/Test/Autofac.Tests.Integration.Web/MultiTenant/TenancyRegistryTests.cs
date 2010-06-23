using System;
using Autofac.Integration.Web.MultiTenant;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Web.MultiTenant
{
    [TestFixture]
    public class TenancyRegistryTests
    {
        class UnidentifiedTenant : ITenantIdentificationPolicy
        {
            public bool TryIdentifyTenant(out object tenantId)
            {
                tenantId = null;
                return false;
            }
        }

        class IdentifiedTenant : ITenantIdentificationPolicy
        {
            readonly object _tenantId;

            public IdentifiedTenant()
                : this(new object())
            {}

            public IdentifiedTenant(object tenantId)
            {
                _tenantId = tenantId;
            }

            public bool TryIdentifyTenant(out object tenantId)
            {
                tenantId = _tenantId;
                return true;
            }

            public object TenantId { get { return _tenantId; } }
        }

        static ILifetimeScope GetScopeWithObjectInBaseConfiguration(ITenantIdentificationPolicy tenantIdentificationPolicy)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            var container = builder.Build();
            var tr = new TenancyRegistry(container);
            return tr.GetTenantApplicationScope(tenantIdentificationPolicy);
        }

        [Test]
        public void AComponentInTheBaseConfigurationIsAvailableToAnUnidentifiedTenant()
        {
            var ut = new UnidentifiedTenant();
            var ts = GetScopeWithObjectInBaseConfiguration(ut);
            Assert.IsTrue(ts.IsRegistered<object>());
        }

        [Test]
        public void AComponentInTheBaseConfigurationIsAvailableToAnIdentitfiedTenant()
        {
            var it = new IdentifiedTenant();
            var ts = GetScopeWithObjectInBaseConfiguration(it);
            Assert.IsTrue(ts.IsRegistered<object>());
        }

        static ILifetimeScope GetScopeWithObjectInDefaultConfiguration(ITenantIdentificationPolicy tenantIdentificationPolicy)
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var tr = new TenancyRegistry(container);
            tr.ConfigureDefaultTenant(dt => dt.RegisterType<object>());
            return tr.GetTenantApplicationScope(tenantIdentificationPolicy);
        }

        [Test]
        public void AComponentInTheDefaultConfigurationIsAvailableToAnUnidentifiedTenant()
        {
            var ut = new UnidentifiedTenant();
            var ts = GetScopeWithObjectInDefaultConfiguration(ut);
            Assert.IsTrue(ts.IsRegistered<object>());
        }

        [Test]
        public void AComponentInTheDefaultConfigurationIsAvailableToAnIdentifiedButUnknownTenant()
        {
            var it = new IdentifiedTenant();
            var ts = GetScopeWithObjectInDefaultConfiguration(it);
            Assert.IsTrue(ts.IsRegistered<object>());
        }

        [Test]
        public void AComponentInTheDefaultConfigurationIsNotAvailableToAKnownTenant()
        {
            var it = new IdentifiedTenant();
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var tr = new TenancyRegistry(container);
            tr.ConfigureTenant(it.TenantId, t => { });
            tr.ConfigureDefaultTenant(t => t.RegisterType<object>());
            var ts = tr.GetTenantApplicationScope(it);
            Assert.IsFalse(ts.IsRegistered<object>());
        }

        static ILifetimeScope GetScopeWithObjectInTenantConfiguration(object tenantId, ITenantIdentificationPolicy tenantIdentificationPolicy)
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var tr = new TenancyRegistry(container);
            tr.ConfigureTenant(tenantId, t => t.RegisterType<object>());
            return tr.GetTenantApplicationScope(tenantIdentificationPolicy);
        }

        [Test]
        public void AComponentInATenantConfigurationIsNotAvailableToAnUnidentifiedTenant()
        {
            var ut = new UnidentifiedTenant();
            var ts = GetScopeWithObjectInTenantConfiguration(new object(), ut);
            Assert.IsFalse(ts.IsRegistered<object>());
        }

        [Test]
        public void AComponentInATenantConfigurationIsAvailableToTheSpecificTenant()
        {
            var it = new IdentifiedTenant();
            var ts = GetScopeWithObjectInTenantConfiguration(it.TenantId, it);
            Assert.IsTrue(ts.IsRegistered<object>());
        }

        [Test]
        public void AComponentInATenantConfigurationIsNotAvailableAnotherSpecificTenant()
        {
            var it = new IdentifiedTenant();
            var ts = GetScopeWithObjectInTenantConfiguration(new object(), it);
            Assert.IsFalse(ts.IsRegistered<object>());
        }
    }
}
