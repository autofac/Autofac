// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core.Registration;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class NestedScopeRegistrationTests
    {
        private interface IMyService
        {
        }

        private interface IServiceA
        {
        }

        private interface IServiceB
        {
        }

        private interface IServiceCommon
        {
        }

        [Fact]
        public void BothLocalAndParentRegistrationsAreAvailable()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<MyComponent>();
            var container = cb.Build();
            var ls1 = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>());
            var ls2 = ls1.BeginLifetimeScope(b => b.RegisterType<MyComponent>());
            Assert.Equal(3, ls2.Resolve<IEnumerable<MyComponent>>().Count());
        }

        [Fact]
        public void BothLocalAndParentRegistrationsAreAvailableViaAdapter()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<MyComponent>();
            var container = cb.Build();
            var ls1 = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>());
            var ls2 = ls1.BeginLifetimeScope(b => b.RegisterType<MyComponent>());
            Assert.Equal(3, ls2.Resolve<IEnumerable<Func<MyComponent>>>().Count());
        }

        [Fact]
        public void ComponentsInNestedLifetimeCanResolveDependenciesFromParent()
        {
            var level1Scope = new ContainerBuilder().Build();
            var level2Scope = level1Scope.BeginLifetimeScope(cb => cb.RegisterType<AddressBook>());
            var level3Scope = level2Scope.BeginLifetimeScope(cb => cb.RegisterType<Person>());
            Assert.NotNull(level3Scope.Resolve<AddressBook>().Add());
        }

        [Fact]
        public void ExplicitCollectionRegistrationsMadeInParentArePreservedInChildScope()
        {
            var obs = new MyComponent[5];
            var cb = new ContainerBuilder();
            cb.RegisterInstance(obs).As<IEnumerable<MyComponent>>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>());
            Assert.Same(obs, ls.Resolve<IEnumerable<MyComponent>>());
        }

        [Fact]
        public void InstancesRegisteredInNestedScopeAreSingletonsInThatScope()
        {
            var rootScope = new ContainerBuilder().Build();
            var dt = new DisposeTracker();
            var nestedScope = rootScope.BeginLifetimeScope(cb => cb.RegisterInstance(dt));
            var dt1 = nestedScope.Resolve<DisposeTracker>();
            Assert.Same(dt, dt1);
        }

        [Fact]
        public void IntermediateRegistrationOverridesParentAsDefault()
        {
            var o1 = new MyComponent();
            var o2 = new MyComponent();

            var builder = new ContainerBuilder();
            builder.Register(c => o1);
            var scope1 = builder.Build();
            var scope2 = scope1.BeginLifetimeScope(b => b.Register(c => o2));
            var scope3 = scope2.BeginLifetimeScope(b => { });

            Assert.Same(o2, scope3.Resolve<MyComponent>());
        }

        [Fact]
        public void LocalRegistrationCanPreserveParentAsDefault()
        {
            var o = new MyComponent();
            var cb = new ContainerBuilder();
            cb.RegisterType<MyComponent>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => b.Register(c => o).PreserveExistingDefaults());
            Assert.NotSame(o, ls.Resolve<MyComponent>());
        }

        [Fact]
        public void MostLocalRegistrationIsDefault()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<MyComponent>();
            var container = cb.Build();
            var ls1 = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>());
            var o = new MyComponent();
            var ls2 = ls1.BeginLifetimeScope(b => b.RegisterInstance(o));
            Assert.Same(o, ls2.Resolve<MyComponent>());
        }

        [Fact]
        public void RegistrationsMadeInLifetimeScopeAreAdapted()
        {
            var container = new ContainerBuilder().Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>().As<IMyService>());

            var component = ls.Resolve<Func<IMyService>>().Invoke();
            Assert.IsType<MyComponent>(component);
        }

        [Fact]
        public void RegistrationsMadeInLifetimeScopeCanBeResolvedThere()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>().As<IMyService>());

            var component = ls.Resolve<IMyService>();
            Assert.IsType<MyComponent>(component);
        }

        [Fact]
        public void RegistrationsMadeInLifetimeScopeCannotBeResolvedInItsParent()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>().As<IMyService>());

            Assert.Throws<ComponentNotRegisteredException>(() => container.Resolve<IMyService>());
        }

        [Fact]
        public void RegistrationsMadeInParentScopeAreAdapted()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<MyComponent>().As<IMyService>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => { });

            var component = container.Resolve<Func<IMyService>>().Invoke();
            Assert.IsType<MyComponent>(component);
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

        [Fact]
        public void WhenRegisteringIntoADeeplyNestedLifetimeScopeParentRegistrationsAreNotDuplicated()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<string>();
            var container = builder.Build();
            var child1 = container.BeginLifetimeScope();
            var child2 = child1.BeginLifetimeScope(b => b.RegisterType<object>());
            Assert.Single(child2.Resolve<IEnumerable<string>>());
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

        private class MyComponent : IMyService
        {
        }

        public class Person
        {
        }

        private class ServiceA : IServiceA, IServiceCommon
        {
        }

        private class ServiceB1 : IServiceB, IServiceCommon
        {
        }

        private class ServiceB2 : IServiceB
        {
        }
    }
}
