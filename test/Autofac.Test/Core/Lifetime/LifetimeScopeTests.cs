using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Test.Scenarios.RegistrationSources;
using Xunit;

namespace Autofac.Test.Core.Lifetime
{
    public class LifetimeScopeTests
    {
        [Fact]
        public void RegistrationsMadeInLifetimeScopeCanBeResolvedThere()
        {
            var container = new ContainerBuilder().Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<object>());
            ls.AssertRegistered<object>();
        }

        [Fact]
        public void RegistrationsMadeInLifetimeScopeCannotBeResolvedInItsParent()
        {
            var container = new ContainerBuilder().Build();
            container.BeginLifetimeScope(b => b.RegisterType<object>());
            container.AssertNotRegistered<object>();
        }

        [Fact]
        public void RegistrationsMadeInLifetimeScopeAreAdapted()
        {
            var container = new ContainerBuilder().Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<object>());
            ls.AssertRegistered<Func<object>>();
        }

        [Fact]
        public void RegistrationsMadeInParentScopeAreAdapted()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => { });
            ls.AssertRegistered<Func<object>>();
        }

        [Fact]
        public void BothLocalAndParentRegistrationsAreAvailable()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls1 = container.BeginLifetimeScope(b => b.RegisterType<object>());
            var ls2 = ls1.BeginLifetimeScope(b => b.RegisterType<object>());
            Assert.Equal(3, ls2.Resolve<IEnumerable<object>>().Count());
        }

        [Fact]
        public void MostLocalRegistrationIsDefault()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls1 = container.BeginLifetimeScope(b => b.RegisterType<object>());
            var o = new object();
            var ls2 = ls1.BeginLifetimeScope(b => b.RegisterInstance(o));
            Assert.Same(o, ls2.Resolve<object>());
        }

        [Fact]
        public void BothLocalAndParentRegistrationsAreAvailableViaAdapter()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls1 = container.BeginLifetimeScope(b => b.RegisterType<object>());
            var ls2 = ls1.BeginLifetimeScope(b => b.RegisterType<object>());
            Assert.Equal(3, ls2.Resolve<IEnumerable<Func<object>>>().Count());
        }

        [Fact]
        public void LocalRegistrationOverridesParentAsDefault()
        {
            var o = new object();
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => b.Register(c => o));
            Assert.Same(o, ls.Resolve<object>());
        }

        [Fact]
        public void IntermediateRegistrationOverridesParentAsDefault()
        {
            var o1 = new object();
            var o2 = new object();

            var builder = new ContainerBuilder();
            builder.Register(c => o1);
            var scope1 = builder.Build();
            var scope2 = scope1.BeginLifetimeScope(b => b.Register(c => o2));
            var scope3 = scope2.BeginLifetimeScope(b => { });

            Assert.Same(o2, scope3.Resolve<object>());
        }

        [Fact]
        public void LocalRegistrationCanPreserveParentAsDefault()
        {
            var o = new object();
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => b.Register(c => o).PreserveExistingDefaults());
            Assert.NotSame(o, ls.Resolve<object>());
        }

        [Fact]
        public void ExplicitCollectionRegistrationsMadeInParentArePreservedInChildScope()
        {
            var obs = new object[5];
            var cb = new ContainerBuilder();
            cb.RegisterInstance(obs).As<IEnumerable<object>>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<object>());
            Assert.Same(obs, ls.Resolve<IEnumerable<object>>());
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
        public void TwoRegistrationsSameServicesDifferentLifetimeScopes()
        {
            // Issue #511
            var builder = new ContainerBuilder();
            builder.RegisterType<ServiceB1>().As<IServiceB>().As<IServiceCommon>();
            builder.RegisterType<ServiceB1>().As<IServiceB>().InstancePerLifetimeScope();
            using (var container = builder.Build())
            {
                using (var lifetimeScope = container.BeginLifetimeScope())
                {
                    var obj1 = lifetimeScope.Resolve<IServiceB>();
                    var obj2 = lifetimeScope.Resolve<IServiceB>();
                    Assert.Same(obj1, obj2);
                }

                using (var lifetimeScope = container.BeginLifetimeScope(x => { }))
                {
                    // Issue #511 mentions that passing a lambda configuration
                    // expression causes the test to fail.
                    var obj1 = lifetimeScope.Resolve<IServiceB>();
                    var obj2 = lifetimeScope.Resolve<IServiceB>();
                    Assert.Same(obj1, obj2);
                }
            }
        }

        public class Person
        {
        }

        public class AddressBook
        {
            private readonly Func<Person> _partyFactory;

            public AddressBook(Func<Person> partyFactory)
            {
                _partyFactory = partyFactory;
            }

            public Person Add()
            {
                return _partyFactory();
            }
        }

        [Fact]
        public void ComponentsInNestedLifetimeCanResolveDependenciesFromParent()
        {
            var level1Scope = new ContainerBuilder().Build();

            var level2Scope = level1Scope.BeginLifetimeScope(cb =>
                cb.RegisterType<AddressBook>());

            var level3Scope = level2Scope.BeginLifetimeScope(cb =>
                cb.RegisterType<Person>());

            level3Scope.Resolve<AddressBook>().Add();
        }

        [Fact]
        public void InstancesRegisteredInNestedScopeAreSingletonsInThatScope()
        {
            var rootScope = new ContainerBuilder().Build();

            var dt = new DisposeTracker();

            var nestedScope = rootScope.BeginLifetimeScope(cb =>
                 cb.RegisterInstance(dt));

            var dt1 = nestedScope.Resolve<DisposeTracker>();
            Assert.Same(dt, dt1);
        }

        [Fact]
        public void SingletonsRegisteredInNestedScopeAreTiedToThatScope()
        {
            var rootScope = new ContainerBuilder().Build();

            var nestedScope = rootScope.BeginLifetimeScope(cb =>
                cb.RegisterType<DisposeTracker>().SingleInstance());

            var dt = nestedScope.Resolve<DisposeTracker>();
            var dt1 = nestedScope.Resolve<DisposeTracker>();
            Assert.Same(dt, dt1);

            nestedScope.Dispose();

            Assert.True(dt.IsDisposed);
        }

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
        public void InstancesRegisteredInParentScope_ButResolvedInChild_AreDisposedWithChild()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>();
            var parent = builder.Build();
            var child = parent.BeginLifetimeScope(b => { });
            var dt = child.Resolve<DisposeTracker>();
            child.Dispose();
            Assert.True(dt.IsDisposed);
        }

        [Fact]
        public void ResolvingFromAnEndedLifetimeProducesObjectDisposedException()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            var container = builder.Build();
            var lifetime = container.BeginLifetimeScope();
            lifetime.Dispose();
            Assert.Throws<ObjectDisposedException>(() => lifetime.Resolve<object>());
        }

        [Fact]
        public void WhenRegisteringIntoADeeplyNestedLifetimeScopeParentRegistrationsAreNotDuplicated()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<string>();
            var container = builder.Build();
            var child1 = container.BeginLifetimeScope();
            var child2 = child1.BeginLifetimeScope(b => b.RegisterType<object>());
            Assert.Equal(1, child2.Resolve<IEnumerable<string>>().Count());
        }

        public interface IServiceA
        {
        }

        public interface IServiceB
        {
        }

        public interface IServiceCommon
        {
        }

        public class ServiceA : IServiceA, IServiceCommon
        {
        }

        public class ServiceB1 : IServiceB, IServiceCommon
        {
        }

        public class ServiceB2 : IServiceB
        {
        }

        [Fact]
        public void ServiceOverrideThroughIntermediateScopeIsCorrect()
        {
            // Issue #475
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(ServiceA)).AsImplementedInterfaces();
            builder.RegisterType(typeof(ServiceB1)).AsImplementedInterfaces();

            using (var scope1 = builder.Build())
            {
                // Scope 1 (Container) resolves default values.
                var service1A = scope1.Resolve<IServiceA>();
                var service1B = scope1.Resolve<IServiceB>();
                Assert.IsType<ServiceA>(service1A);
                Assert.IsType<ServiceB1>(service1B);

                using (var scope2 = scope1.BeginLifetimeScope(cb =>
                    cb.RegisterType(typeof(ServiceB2))
                        .AsImplementedInterfaces()
                        .InstancePerLifetimeScope()))
                {
                    // Scope 2 overrides the registration for one service
                    // but leaves the other in place.
                    var service2A = scope2.Resolve<IServiceA>();
                    var service2B = scope2.Resolve<IServiceB>();
                    Assert.IsType<ServiceA>(service2A);
                    Assert.IsType<ServiceB2>(service2B);

                    using (var scope3 = scope2.BeginLifetimeScope(cb => { }))
                    {
                        // Scope 3 provides an empty set of registrations
                        // and should retain the overrides from scope 2.
                        var service3A = scope3.Resolve<IServiceA>();
                        var service3B = scope3.Resolve<IServiceB>();
                        Assert.IsType<ServiceA>(service3A);
                        Assert.IsType<ServiceB2>(service3B);
                    }
                }
            }
        }

        private class AThatDependsOnB
        {
            public AThatDependsOnB(BThatCreatesA bThatCreatesA)
            {
            }
        }

        private class BThatCreatesA
        {
            public BThatCreatesA(Func<BThatCreatesA, AThatDependsOnB> factory)
            {
                factory(this);
            }
        }

        [Fact]
        public void InstancePerLifetimeScopeServiceCannotCreateSecondInstanceOfSelfDuringConstruction()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AThatDependsOnB>().InstancePerLifetimeScope();
            builder.RegisterType<BThatCreatesA>().InstancePerLifetimeScope();
            var container = builder.Build();

            var exception = Assert.Throws<DependencyResolutionException>(() => container.Resolve<AThatDependsOnB>());

            Assert.Equal(exception.Message, string.Format(CultureInfo.CurrentCulture, LifetimeScopeResources.SelfConstructingDependencyDetected, typeof(AThatDependsOnB).FullName));
        }

        internal class DependsOnRegisteredInstance
        {
            internal object Instance { get; set; }

            public DependsOnRegisteredInstance(object instance)
            {
                Instance = instance;
            }
        }

        internal class UpdatesRegistryWithInstance
        {
            private readonly IComponentContext _registerContext;

            public UpdatesRegistryWithInstance(IComponentContext registerContext)
            {
                _registerContext = registerContext;
            }

            internal void UpdateRegistry(object instance)
            {
                var builder = new ContainerBuilder();
                builder.RegisterInstance(instance);
                builder.UpdateRegistry(_registerContext.ComponentRegistry);
            }
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
        public void RegistrationsCanUseContextProperties()
        {
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 0;
            builder.Register(ctx =>
            {
                return ctx.ComponentRegistry.Properties["count"].ToString();
            }).As<string>();

            var container = builder.Build();

            Assert.Equal("0", container.Resolve<string>());
            using (var scope = container.BeginLifetimeScope(b => b.Properties["count"] = 1))
            {
                Assert.Equal("1", scope.Resolve<string>());
            }

            Assert.Equal("0", container.Resolve<string>());
        }

        [Fact]
        public void ChildScopeBuilderGetsParentProperties()
        {
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 5;
            var container = builder.Build();

            using (var outerScope = container.BeginLifetimeScope(b =>
            {
                Assert.Equal(5, b.Properties["count"]);
                b.Properties["count"] = 10;
            }))
            {
                Assert.Equal(10, outerScope.ComponentRegistry.Properties["count"]);
                using (var innerScope = outerScope.BeginLifetimeScope(b =>
                {
                    Assert.Equal(10, b.Properties["count"]);
                    b.Properties["count"] = 15;
                }))
                {
                    Assert.Equal(5, container.ComponentRegistry.Properties["count"]);
                    Assert.Equal(10, outerScope.ComponentRegistry.Properties["count"]);
                    Assert.Equal(15, innerScope.ComponentRegistry.Properties["count"]);
                }
            }
        }

        [Fact]
        public void LambdaRegistrationsDoNotAffectPropertyPropagation()
        {
            // In the past we've had issues where lambda configuration
            // expressions change the behavior of builder/container semantics.
            // This ensures we can use properties even when we aren't using
            // lambdas in lifetime scope startup.
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 5;
            var container = builder.Build();

            using (var outerScope = container.BeginLifetimeScope())
            {
                using (var innerScope = outerScope.BeginLifetimeScope(b =>
                {
                    b.Properties["count"] = 15;
                }))
                {
                    Assert.Equal(5, container.ComponentRegistry.Properties["count"]);
                    Assert.Equal(5, outerScope.ComponentRegistry.Properties["count"]);
                    Assert.Equal(15, innerScope.ComponentRegistry.Properties["count"]);
                }
            }
        }

        public class HandlerException : Exception
        {
        }

        [Fact]
        public void ContainerIsDisposedEvenIfHandlerThrowsException()
        {
            var rootScope = new ContainerBuilder().Build();

            var nestedScope = rootScope.BeginLifetimeScope(cb =>
                cb.RegisterType<DisposeTracker>().SingleInstance());

            nestedScope.CurrentScopeEnding += (sender, args) => throw new HandlerException();

            var dt = nestedScope.Resolve<DisposeTracker>();

            try
            {
                nestedScope.Dispose();
            }
            catch (HandlerException)
            {
            }

            Assert.True(dt.IsDisposed);
        }
    }
}
