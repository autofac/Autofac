using System;
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
            var parent = new Container();
            parent.ComponentRegistry.AddRegistrationSource(new ObjectRegistrationSource(parentInstance));
            var child = parent.BeginLifetimeScope(builder =>
                    builder.RegisterSource(new ObjectRegistrationSource(childInstance)));
            var fromChild = child.Resolve<object>();
            Assert.Same(childInstance, fromChild);
        }

        [Fact]
        public void CanRegisterInstanceUsingUpdateInsideChildLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<UpdatesRegistryWithInstance>();
            builder.RegisterType<DependsOnRegisteredInstance>();
            var container = builder.Build();

            var scope = container.BeginLifetimeScope();
            var updatesRegistry = scope.Resolve<UpdatesRegistryWithInstance>();
            updatesRegistry.UpdateRegistry(new object());
            var instance1 = scope.Resolve<DependsOnRegisteredInstance>();

            scope = container.BeginLifetimeScope();
            updatesRegistry = scope.Resolve<UpdatesRegistryWithInstance>();
            updatesRegistry.UpdateRegistry(new object());
            var instance2 = scope.Resolve<DependsOnRegisteredInstance>();

            Assert.NotSame(instance1, instance2);
        }

        [Fact]
        public void LifetimeScopeCreatedWithAdditionalRegistrationsUsesScopeRestrictedRegistry()
        {
            var rootScope = new ContainerBuilder().Build();

            var nestedScope = rootScope.BeginLifetimeScope(cb =>
                cb.RegisterType<object>().SingleInstance());

            Assert.IsType<ScopeRestrictedRegistry>(nestedScope.ComponentRegistry);
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
            var child = container.BeginLifetimeScope();
            child.ComponentRegistry.Register(nestedRegistration);
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

        internal class UpdatesRegistryWithInstance
        {
            private readonly IComponentContext _registerContext;

            public UpdatesRegistryWithInstance(IComponentContext registerContext)
            {
                this._registerContext = registerContext;
            }

            internal void UpdateRegistry(object instance)
            {
                var builder = new ContainerBuilder();
                builder.RegisterInstance(instance);
                builder.UpdateRegistry(this._registerContext.ComponentRegistry);
            }
        }
    }
}
