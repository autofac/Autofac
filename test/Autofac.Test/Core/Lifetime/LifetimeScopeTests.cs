using System;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Test.Scenarios.RegistrationSources;
using Autofac.Test.Util;
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

            var builder = new ContainerBuilder();
            builder.ComponentRegistryBuilder.AddRegistrationSource(new ObjectRegistrationSource(parentInstance));

            var parent = builder.Build();
            var child = parent.BeginLifetimeScope(lifetimeScopeBuilder =>
                    lifetimeScopeBuilder.RegisterSource(new ObjectRegistrationSource(childInstance)));
            var fromChild = child.Resolve<object>();
            Assert.Same(childInstance, fromChild);
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
            var child = container.BeginLifetimeScope(x => x.ComponentRegistryBuilder.Register(nestedRegistration));
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

        [Fact]
        public async ValueTask AsyncDisposeLifetimeScopeDisposesRegistrationsAsync()
        {
            var cb = new ContainerBuilder();

            cb.RegisterType<DisposeTracker>().InstancePerLifetimeScope().AsSelf();
            cb.RegisterType<AsyncDisposeTracker>().InstancePerLifetimeScope().AsSelf();
            cb.RegisterType<AsyncOnlyDisposeTracker>().InstancePerLifetimeScope().AsSelf();

            var container = cb.Build();

            DisposeTracker tracker;
            AsyncDisposeTracker asyncTracker;
            AsyncOnlyDisposeTracker asyncOnlyTracker;

            await using (var scope = container.BeginLifetimeScope())
            {
                tracker = scope.Resolve<DisposeTracker>();
                asyncTracker = scope.Resolve<AsyncDisposeTracker>();
                asyncOnlyTracker = scope.Resolve<AsyncOnlyDisposeTracker>();

                Assert.False(tracker.IsDisposed);
                Assert.False(asyncTracker.IsSyncDisposed);
                Assert.False(asyncTracker.IsAsyncDisposed);
                Assert.False(asyncOnlyTracker.IsAsyncDisposed);
            }

            Assert.True(tracker.IsDisposed);
            Assert.True(asyncTracker.IsAsyncDisposed);
            Assert.True(asyncOnlyTracker.IsAsyncDisposed);
            Assert.False(asyncTracker.IsSyncDisposed);
        }

        [Fact]
        public void DisposeLifetimeScopeDisposesRegistrationsThatAreAsyncAndSyncDispose()
        {
            var cb = new ContainerBuilder();

            cb.RegisterType<DisposeTracker>().InstancePerLifetimeScope().AsSelf();
            cb.RegisterType<AsyncDisposeTracker>().InstancePerLifetimeScope().AsSelf();

            var container = cb.Build();

            DisposeTracker tracker;
            AsyncDisposeTracker asyncTracker;

            using (var scope = container.BeginLifetimeScope())
            {
                tracker = scope.Resolve<DisposeTracker>();
                asyncTracker = scope.Resolve<AsyncDisposeTracker>();

                Assert.False(tracker.IsDisposed);
                Assert.False(asyncTracker.IsSyncDisposed);
                Assert.False(asyncTracker.IsAsyncDisposed);
            }

            Assert.True(tracker.IsDisposed);
            Assert.False(asyncTracker.IsAsyncDisposed);
            Assert.True(asyncTracker.IsSyncDisposed);
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
