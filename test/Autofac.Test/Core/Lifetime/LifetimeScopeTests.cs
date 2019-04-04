using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Test.Scenarios.RegistrationSources;
using Xunit;

namespace Autofac.Test.Core.Lifetime
{
    public class LifetimeScopeTests
    {
        [Fact]
        public void AdaptersInNestedScopeOverrideAdaptersInParent()
        {
            const string parentInstance = "p";
            const string childInstance = "c";

            IComponentRegistryBuilder componentRegistryBuilder = new ComponentRegistry(new Dictionary<string, object>());
            componentRegistryBuilder.AddRegistrationSource(new ObjectRegistrationSource(parentInstance));
            var registry = componentRegistryBuilder.Build();

            var parent = new Container(registry);
            var child = parent.BeginLifetimeScope(builder =>
                    builder.RegisterSource(new ObjectRegistrationSource(childInstance)));
            var fromChild = child.Resolve<object>();
            Assert.Same(childInstance, fromChild);
        }

        [Fact]
        public void LifetimeScopeCreatedWithAdditionalRegistrationsUsesScopeRestrictedRegistry()
        {
            var rootScope = new ContainerBuilder().Build();

            var nestedScope = rootScope.BeginLifetimeScope(cb =>
                cb.RegisterType<object>().SingleInstance());

            Assert.IsType<ScopeRestrictedRegistryBuilder>(nestedScope.ComponentRegistry);
        }

        [Fact]
        public void NestedComponentRegistryIsProperlyDisposed()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var nestedRegistration = Mocks.GetComponentRegistration();
            var child = container.BeginLifetimeScope(b => b.RegisterComponent(nestedRegistration));
            child.Dispose();
            Assert.True(nestedRegistration.IsDisposed);
        }

        [Fact]
        public void NestedComponentRegistryIsProperlyDisposedEvenWhenRegistryUpdatedLater()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var nestedRegistration = Mocks.GetComponentRegistration();
            var child = container.BeginLifetimeScope(x => x.RegisterComponent(nestedRegistration));
            child.Dispose();
            Assert.True(nestedRegistration.IsDisposed);
        }

        [Fact]
        public void NestedLifetimeScopesMaintainServiceLimitTypes()
        {
            // Issue #365
            var cb = new ContainerBuilder();
            cb.RegisterType<Person>();
            var container = cb.Build();
            var service = new TypedService(typeof(Person));
            using (var unconfigured = container.BeginLifetimeScope())
            {
                IComponentRegistration reg = null;
                Assert.True(unconfigured.ComponentRegistry.TryGetRegistration(service, out reg), "The registration should have been found in the unconfigured scope.");
                Assert.Equal(typeof(Person), reg.Activator.LimitType);
            }

            using (var configured = container.BeginLifetimeScope(b => { }))
            {
                IComponentRegistration reg = null;
                Assert.True(configured.ComponentRegistry.TryGetRegistration(service, out reg), "The registration should have been found in the configured scope.");
                Assert.Equal(typeof(Person), reg.Activator.LimitType);
            }
        }

        internal class DependsOnRegisteredInstance
        {
            public DependsOnRegisteredInstance(object instance)
            {
                this.Instance = instance;
            }

            internal object Instance { get; set; }
        }

        public class HandlerException : Exception
        {
        }

        public class Person
        {
        }
    }
}
