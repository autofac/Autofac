using System;
using Autofac;
using AutofacContrib.Multitenant;
using AutofacContrib.Tests.Multitenant.Stubs;
using NUnit.Framework;

namespace AutofacContrib.Tests.Multitenant
{
    [TestFixture]
    public class MultitenantContainerFixture
    {
        [Test(Description = "BeginLifetimeScope should allow a sub-scope to be configured.")]
        public void BeginLifetimeScope_ChildScopeCanBeConfigured()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency1Impl1>().As<IStubDependency1>());
            using (var nestedScope = mtc.BeginLifetimeScope(b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>()))
            {
                var nestedDependency = nestedScope.Resolve<IStubDependency1>();
                Assert.IsInstanceOf<StubDependency1Impl2>(nestedDependency, "The child scope was not properly configured.");
            }
        }

        [Test(Description = "BeginLifetimeScope should allow a sub-scope to be configured and tagged.")]
        public void BeginLifetimeScope_ChildScopeCanBeConfiguredAndTagged()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency1Impl1>().As<IStubDependency1>());
            using (var nestedScope = mtc.BeginLifetimeScope("tag", b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>()))
            {
                Assert.AreEqual("tag", nestedScope.Tag, "The child scope could not be tagged.");
                var nestedDependency = nestedScope.Resolve<IStubDependency1>();
                Assert.IsInstanceOf<StubDependency1Impl2>(nestedDependency, "The child scope was not properly configured.");
            }
        }

        [Test(Description = "BeginLifetimeScope should allow a sub-scope to be tagged.")]
        public void BeginLifetimeScope_ChildScopeCanBeTagged()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            using (var nestedScope = mtc.BeginLifetimeScope("tag"))
            {
                Assert.AreEqual("tag", nestedScope.Tag, "The child scope could not be tagged.");
            }
        }

        [Test(Description = "BeginLifetimeScope should begin a lifetime scope based on the current tenant scope.")]
        public void BeginLifetimeScope_CreatesLifetimeScopeForCurrentTenant()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>().InstancePerLifetimeScope());
            var tenantScope = mtc.GetCurrentTenantScope();
            var tenantDependency = tenantScope.Resolve<IStubDependency1>();
            using (var nestedScope = mtc.BeginLifetimeScope())
            {
                var nestedDependency = nestedScope.Resolve<IStubDependency1>();
                Assert.AreNotSame(tenantDependency, nestedDependency, "The dependency should be registered, but the scope should resolve a new instance.");
            }
        }

        [Test(Description = "The ComponentRegistry property should return the registry from the current tenant's lifetime scope.")]
        public void ComponentRegistry_ReturnsRegistryFromCurrentTenantLifetimeScope()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            var scope = mtc.GetCurrentTenantScope();
            Assert.AreSame(scope.ComponentRegistry, mtc.ComponentRegistry, "The ComponentRegistry property should behave based on context.");
        }

        [Test(Description = "Once the default tenant has been configured, you can't change the configuration.")]
        public void ConfigureTenant_DoesNotAllowMultipleSubsequentRegistrationsForDefaultTenant()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubDependency1Impl1>().As<IStubDependency1>();
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, builder.Build());
            mtc.ConfigureTenant(null, b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>());
            Assert.Throws<InvalidOperationException>(() => mtc.ConfigureTenant(null, b => b.RegisterType<StubDependency2Impl2>().As<IStubDependency2>()));
        }

        [Test(Description = "Once a tenant has been configured, you can't change the configuration.")]
        public void ConfigureTenant_DoesNotAllowMultipleSubsequentRegistrationsForOneTenant()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubDependency1Impl1>().As<IStubDependency1>();
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, builder.Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>());
            Assert.Throws<InvalidOperationException>(() => mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency2Impl2>().As<IStubDependency2>()));
        }

        [Test(Description = "Configuring a tenant requires that you provide a configuration lambda.")]
        public void ConfigureTenant_RequiresConfiguration()
        {
            var builder = new ContainerBuilder();
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, builder.Build());
            Assert.Throws<ArgumentNullException>(() => mtc.ConfigureTenant("tenant1", null));
        }

        [Test(Description = "Attempts to create a multitenant container without a tenant ID strategy.")]
        public void Ctor_NullApplicationContainer()
        {
            Assert.Throws<ArgumentNullException>(() => new MultitenantContainer(new StubTenantIdentificationStrategy(), null));
        }

        [Test(Description = "Attempts to create a multitenant container without a tenant ID strategy.")]
        public void Ctor_NullTenantIdentificationStrategy()
        {
            Assert.Throws<ArgumentNullException>(() => new MultitenantContainer(null, new ContainerBuilder().Build()));
        }

        [Test(Description = "Verifies that the properties passed into the constructor are stored for later use.")]
        public void Ctor_SetsProperties()
        {
            var container = new ContainerBuilder().Build();
            var strategy = new StubTenantIdentificationStrategy();
            var mtc = new MultitenantContainer(strategy, container);
            Assert.AreSame(container, mtc.ApplicationContainer, "The application container wasn't set.");
            Assert.AreSame(strategy, mtc.TenantIdentificationStrategy, "The tenant ID strategy wasn't set.");
        }

        [Test(Description = "Disposing the multitenant container should dispose of all of the tenant lifetime scopes.")]
        public void Dispose_DisposesTenantLifetimeScopes()
        {
            var appDependency = new StubDisposableDependency();
            var tenantDependency = new StubDisposableDependency();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(appDependency).OwnedByLifetimeScope();
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, builder.Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterInstance(tenantDependency).OwnedByLifetimeScope());

            // Resolve the tenant dependency so it's added to the list of things to dispose.
            // If you don't do this, it won't be queued for disposal and the test fails.
            mtc.Resolve<StubDisposableDependency>();

            mtc.Dispose();
            Assert.IsTrue(appDependency.IsDisposed, "The application scope didn't run Dispose.");
            Assert.IsTrue(tenantDependency.IsDisposed, "The tenant scope didn't run Dispose.");
        }

        [Test(Description = "The Disposer property should return the disposer from the current tenant's lifetime scope.")]
        public void Disposer_ReturnsRegistryFromCurrentTenantLifetimeScope()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            var scope = mtc.GetCurrentTenantScope();
            Assert.AreSame(scope.Disposer, mtc.Disposer, "The Disposer property should behave based on context.");
        }

        [Test(Description = "As the context changes, so should the current tenant scope.")]
        public void GetCurrentTenantScope_ChangesByContext()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            var tenant1a = mtc.GetCurrentTenantScope();
            strategy.TenantId = "tenant2";
            var tenant2 = mtc.GetCurrentTenantScope();
            strategy.TenantId = "tenant1";
            var tenant1b = mtc.GetCurrentTenantScope();
            Assert.AreSame(tenant1a, tenant1b, "Every time a specific tenant context is seen, the same current scope should be returned.");
            Assert.AreNotSame(tenant1a, tenant2, "When different tenant contexts are seen, different tenant scopes should be returned.");
        }

        [Test(Description = "If the tenant ID is found by the identification strategy, the appropriate scope should be returned.")]
        public void GetCurrentTenantScope_TenantFound()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            var current = mtc.GetCurrentTenantScope();
            var tenant = mtc.GetTenantScope("tenant1");
            Assert.AreSame(tenant, current, "The current scope should be the scope for the identified tenant.");
        }

        [Test(Description = "If the tenant ID is not found by the identification strategy, the default tenant scope should be returned.")]
        public void GetCurrentTenantScope_TenantNotFound()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1",
                IdentificationSuccess = false
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            var current = mtc.GetCurrentTenantScope();
            var tenant = mtc.GetTenantScope(null);
            Assert.AreSame(tenant, current, "The current scope should be the default tenant scope for the unidentified tenant.");
        }

        [Test(Description = "Using null on GetTenantScope should be valid because it refers to the default tenant.")]
        public void GetTenantScope_NullIsDefaultTenant()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            var scope = mtc.GetTenantScope(null);
            Assert.IsNotNull(scope, "The default tenant scope should not be null.");
            Assert.AreNotSame(mtc.ApplicationContainer, scope, "The default tenant scope should be a real scope, not just the application container.");
        }

        [Test(Description = "GetTenantScope should retrieve a scope for a tenant that has been configured.")]
        public void GetTenantScope_GetsTenantScopeForConfiguredTenant()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>());
            var scope = mtc.GetTenantScope("tenant1");
            Assert.IsNotNull(scope, "The tenant scope retrieved not be null.");
            Assert.AreNotSame(mtc.ApplicationContainer, scope, "The tenant scope should be a real scope, not just the application container.");
        }

        [Test(Description = "GetTenantScope should retrieve a scope for a tenant that has not been configured.")]
        public void GetTenantScope_GetsTenantScopeForUnconfiguredTenant()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            var scope = mtc.GetTenantScope("tenant1");
            Assert.IsNotNull(scope, "The tenant scope retrieved not be null.");
            Assert.AreNotSame(mtc.ApplicationContainer, scope, "The tenant scope should be a real scope, not just the application container.");
        }

        [Test(Description = "GetTenantScope should retrieve the same tenant scope for a tenant on subsequent retrievals.")]
        public void GetTenantScope_SubsequentRetrievalsGetTheSameLifetimeScope()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>());
            var scope1 = mtc.GetTenantScope("tenant1");
            var scope2 = mtc.GetTenantScope("tenant1");
            Assert.AreSame(scope1, scope2, "The tenant scope should not change across subsequent retrievals.");
        }

        [Test(Description = "Resolves a dependency that is a singleton at the application level. Verifies lifetime is respected.")]
        public void Resolve_ApplicationLevelSingleton()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubDependency1Impl1>().As<IStubDependency1>().SingleInstance();
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, builder.Build());

            // Two resolutions for a single tenant
            var dep1 = mtc.Resolve<IStubDependency1>();
            var dep2 = mtc.Resolve<IStubDependency1>();

            // One resolution for a different tenant
            strategy.TenantId = "tenant2";
            var dep3 = mtc.Resolve<IStubDependency1>();

            Assert.AreSame(dep1, dep2, "The two dependencies resolved for the first tenant should be the same.");
            Assert.AreSame(dep1, dep3, "The dependencies resolved across tenants should be the same.");
        }

        [Test(Description = "Resolves a dependency and verifies that it is tenant-specific.")]
        public void Resolve_ResolvesTenantSpecificRegistrations()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubDependency1Impl1>().As<IStubDependency1>();
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, builder.Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>());

            Assert.IsInstanceOf<StubDependency1Impl2>(mtc.Resolve<IStubDependency1>(), "The wrong dependency type was resolved for the contextual tenant.");
        }

        [Test(Description = "If tenants don't have dependency overrides, they should fall back to the application container.")]
        public void Resolve_TenantFallbackToApplicationContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubDependency1Impl1>().As<IStubDependency1>();
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, builder.Build());
            Assert.IsInstanceOf<StubDependency1Impl1>(mtc.Resolve<IStubDependency1>(), "The wrong dependency type was resolved for the contextual tenant.");
        }

        [Test(Description = "Resolves a dependency that is a singleton at the tenant level. Verifies lifetime is respected.")]
        public void Resolve_TenantLevelSingleton()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubDependency1Impl1>().As<IStubDependency1>().SingleInstance();

            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, builder.Build());
            mtc.ConfigureTenant("tenant1", b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>().SingleInstance());
            mtc.ConfigureTenant("tenant2", b => b.RegisterType<StubDependency1Impl2>().As<IStubDependency1>().SingleInstance());

            // Get the application-level dependency
            var appLevel = mtc.ApplicationContainer.Resolve<IStubDependency1>();

            // Two resolutions for a single tenant
            var dep1 = mtc.Resolve<IStubDependency1>();
            var dep2 = mtc.Resolve<IStubDependency1>();

            // One resolution for a different tenant
            strategy.TenantId = "tenant2";
            var dep3 = mtc.Resolve<IStubDependency1>();

            Assert.IsInstanceOf<StubDependency1Impl2>(dep1, "Tenant 1's dependency should be the override value.");
            Assert.IsInstanceOf<StubDependency1Impl2>(dep3, "Tenant 2's dependency should be the override value.");
            Assert.IsInstanceOf<StubDependency1Impl1>(appLevel, "The application's dependency should be the base value.");
            Assert.AreSame(dep1, dep2, "The two dependencies resolved for the first tenant should be the same.");
            Assert.AreNotSame(dep1, dep3, "The dependencies resolved across tenants should not be the same.");
            Assert.AreNotSame(dep1, appLevel, "The dependencies resolved at the tenant level should not be the same as the ones at the application level.");
        }

        [Test(Description = "The Tag property should return the tag from the current tenant's lifetime scope.")]
        public void Tag_ReturnsRegistryFromCurrentTenantLifetimeScope()
        {
            var strategy = new StubTenantIdentificationStrategy()
            {
                TenantId = "tenant1"
            };
            var mtc = new MultitenantContainer(strategy, new ContainerBuilder().Build());
            var scope = mtc.GetCurrentTenantScope();
            Assert.AreSame(scope.Tag, mtc.Tag, "The Tag property should behave based on context.");
        }
    }
}
