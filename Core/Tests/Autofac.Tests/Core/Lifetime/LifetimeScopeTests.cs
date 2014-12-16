﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Tests.Scenarios.RegistrationSources;
using NUnit.Framework;

namespace Autofac.Tests.Core.Lifetime
{
    [TestFixture]
    public class LifetimeScopeTests
    {
        [Test]
        public void RegistrationsMadeInLifetimeScopeCanBeResolvedThere()
        {
            var container = new ContainerBuilder().Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<object>());
            ls.AssertRegistered<object>();
        }

        [Test]
        public void RegistrationsMadeInLifetimeScopeCannotBeResolvedInItsParent()
        {
            var container = new ContainerBuilder().Build();
            container.BeginLifetimeScope(b => b.RegisterType<object>());
            container.AssertNotRegistered<object>();
        }

        [Test]
        public void RegistrationsMadeInLifetimeScopeAreAdapted()
        {
            var container = new ContainerBuilder().Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<object>());
            ls.AssertRegistered<Func<object>>();
        }

        [Test]
        public void RegistrationsMadeInParentScopeAreAdapted()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => { });
            ls.AssertRegistered<Func<object>>();
        }

        [Test]
        public void BothLocalAndParentRegistrationsAreAvailable()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls1 = container.BeginLifetimeScope(b => b.RegisterType<object>());
            var ls2 = ls1.BeginLifetimeScope(b => b.RegisterType<object>());
            Assert.AreEqual(3, ls2.Resolve<IEnumerable<object>>().Count());
        }

        [Test]
        public void MostLocalRegistrationIsDefault()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls1 = container.BeginLifetimeScope(b => b.RegisterType<object>());
            var o = new object();
            var ls2 = ls1.BeginLifetimeScope(b => b.RegisterInstance(o));
            Assert.AreSame(o, ls2.Resolve<object>());
        }

        [Test]
        public void BothLocalAndParentRegistrationsAreAvailableViaAdapter()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls1 = container.BeginLifetimeScope(b => b.RegisterType<object>());
            var ls2 = ls1.BeginLifetimeScope(b => b.RegisterType<object>());
            Assert.AreEqual(3, ls2.Resolve<IEnumerable<Func<object>>>().Count());
        }

        [Test]
        public void LocalRegistrationOverridesParentAsDefault()
        {
            var o = new object();
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => b.Register(c => o));
            Assert.AreSame(o, ls.Resolve<object>());
        }

        [Test]
        public void IntermediateRegistrationOverridesParentAsDefault()
        {
            var o1 = new object();
            var o2 = new object();

            var builder = new ContainerBuilder();
            builder.Register(c => o1);
            var scope1 = builder.Build();
            var scope2 = scope1.BeginLifetimeScope(b => b.Register(c => o2));
            var scope3 = scope2.BeginLifetimeScope(b => { });

            Assert.AreSame(o2, scope3.Resolve<object>());
        }

        [Test]
        public void LocalRegistrationCanPreserveParentAsDefault()
        {
            var o = new object();
            var cb = new ContainerBuilder();
            cb.RegisterType<object>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => b.Register(c => o).PreserveExistingDefaults());
            Assert.AreNotSame(o, ls.Resolve<object>());
        }

        [Test]
        public void ExplicitCollectionRegistrationsMadeInParentArePreservedInChildScope()
        {
            var obs = new object[5];
            var cb = new ContainerBuilder();
            cb.RegisterInstance(obs).As<IEnumerable<object>>();
            var container = cb.Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<object>());
            Assert.AreSame(obs, ls.Resolve<IEnumerable<object>>());
        }

        [Test(Description = "Issue #365")]
        public void NestedLifetimeScopesMaintainServiceLimitTypes()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<Person>();
            var container = cb.Build();
            var service = new TypedService(typeof(Person));
            using (var unconfigured = container.BeginLifetimeScope())
            {
                IComponentRegistration reg = null;
                Assert.IsTrue(unconfigured.ComponentRegistry.TryGetRegistration(service, out reg), "The registration should have been found in the unconfigured scope.");
                Assert.AreEqual(typeof(Person), reg.Activator.LimitType, "The limit type on the registration in the unconfigured scope was changed.");
            }
            using (var configured = container.BeginLifetimeScope(b => { }))
            {
                IComponentRegistration reg = null;
                Assert.IsTrue(configured.ComponentRegistry.TryGetRegistration(service, out reg), "The registration should have been found in the configured scope.");
                Assert.AreEqual(typeof(Person), reg.Activator.LimitType, "The limit type on the registration in the configured scope was changed.");
            }
        }

        [Test(Description = "Issue #397")]
        [Ignore("Issue #397 needs to be well thought-out due to possible memory leaks. Initial fix rolled out.")]
        public void NestedLifetimeScopesGetDisposedWhenParentIsDisposed()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var l1 = container.BeginLifetimeScope();
            var l2 = l1.BeginLifetimeScope();
            var L3 = l2.BeginLifetimeScope(b => b.RegisterType<DisposeTracker>());
            var tracker = L3.Resolve<DisposeTracker>();
            Assert.IsFalse(tracker.IsDisposed, "The tracker should not yet be disposed.");
            container.Dispose();
            Assert.IsTrue(tracker.IsDisposed, "The tracker should have been disposed along with the lifetime scope chain.");
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

        [Test]
        public void ComponentsInNestedLifetimeCanResolveDependenciesFromParent()
        {
            var level1Scope = new ContainerBuilder().Build();

            var level2Scope = level1Scope.BeginLifetimeScope(cb =>
                cb.RegisterType<AddressBook>());

            var level3Scope = level2Scope.BeginLifetimeScope(cb =>
                cb.RegisterType<Person>());

            level3Scope.Resolve<AddressBook>().Add();
        }

        [Test]
        public void InstancesRegisteredInNestedScopeAreSingletonsInThatScope()
        {
            var rootScope = new ContainerBuilder().Build();

            var dt = new DisposeTracker();

            var nestedScope = rootScope.BeginLifetimeScope(cb =>
                 cb.RegisterInstance(dt));

            var dt1 = nestedScope.Resolve<DisposeTracker>();
            Assert.AreSame(dt, dt1);
        }

        [Test]
        public void SingletonsRegisteredInNestedScopeAreTiedToThatScope()
        {
            var rootScope = new ContainerBuilder().Build();

            var nestedScope = rootScope.BeginLifetimeScope(cb =>
                cb.RegisterType<DisposeTracker>().SingleInstance());

            var dt = nestedScope.Resolve<DisposeTracker>();
            var dt1 = nestedScope.Resolve<DisposeTracker>();
            Assert.AreSame(dt, dt1);

            nestedScope.Dispose();

            Assert.That(dt.IsDisposed);
        }

        [Test]
        public void AdaptersInNestedScopeOverrideAdaptersInParent()
        {
            const string parentInstance = "p";
            const string childInstance = "c";
            var parent = new Container();
            parent.ComponentRegistry.AddRegistrationSource(new ObjectRegistrationSource(parentInstance));
            var child = parent.BeginLifetimeScope(builder =>
                    builder.RegisterSource(new ObjectRegistrationSource(childInstance)));
            var fromChild = child.Resolve<object>();
            Assert.AreSame(childInstance, fromChild);
        }

        [Test]
        public void InstancesRegisteredInParentScope_ButResolvedInChild_AreDisposedWithChild()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<DisposeTracker>();
            var parent = builder.Build();
            var child = parent.BeginLifetimeScope(b => { });
            var dt = child.Resolve<DisposeTracker>();
            child.Dispose();
            Assert.That(dt.IsDisposed);
        }

        [Test]
        public void ResolvingFromAnEndedLifetimeProducesObjectDisposedException()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<object>();
            var container = builder.Build();
            var lifetime = container.BeginLifetimeScope();
            lifetime.Dispose();
            Assert.Throws<ObjectDisposedException>(() => lifetime.Resolve<object>());
        }

        [Test]
        public void WhenRegisteringIntoADeeplyNestedLifetimeScopeParentRegistrationsAreNotDuplicated()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<string>();
            var container = builder.Build();
            var child1 = container.BeginLifetimeScope();
            var child2 = child1.BeginLifetimeScope(b => b.RegisterType<object>());
            Assert.AreEqual(1, child2.Resolve<IEnumerable<string>>().Count());
        }

        public interface IServiceA { }
        public interface IServiceB { }
        public interface IServiceCommon { }

        public class ServiceA : IServiceA, IServiceCommon { }
        public class ServiceB1 : IServiceB, IServiceCommon { }
        public class ServiceB2 : IServiceB { }

        [Test(Description = "Issue #475")]
        public void ServiceOverrideThroughIntermediateScopeIsCorrect()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(ServiceA)).AsImplementedInterfaces();
            builder.RegisterType(typeof(ServiceB1)).AsImplementedInterfaces();

            using (var scope1 = builder.Build())
            {
                // Scope 1 (Container) resolves default values.
                var service1A = scope1.Resolve<IServiceA>();
                var service1B = scope1.Resolve<IServiceB>();
                Assert.IsInstanceOf<ServiceA>(service1A, "Default component type for IServiceA service in container was wrong type.");
                Assert.IsInstanceOf<ServiceB1>(service1B, "Default component type for IServiceB service in container was wrong type.");

                using (var scope2 = scope1.BeginLifetimeScope(cb =>
                    cb.RegisterType(typeof(ServiceB2))
                        .AsImplementedInterfaces()
                        .InstancePerLifetimeScope()))
                {
                    // Scope 2 overrides the registration for one service
                    // but leaves the other in place.
                    var service2A = scope2.Resolve<IServiceA>();
                    var service2B = scope2.Resolve<IServiceB>();
                    Assert.IsInstanceOf<ServiceA>(service2A, "Component type for IServiceA service in first child scope should have been default.");
                    Assert.IsInstanceOf<ServiceB2>(service2B, "Component type for IServiceB service in first child scope should have been override.");

                    using (var scope3 = scope2.BeginLifetimeScope(cb => { }))
                    {
                        // Scope 3 provides an empty set of registrations
                        // and should retain the overrides from scope 2.
                        var service3A = scope3.Resolve<IServiceA>();
                        var service3B = scope3.Resolve<IServiceB>();
                        Assert.IsInstanceOf<ServiceA>(service3A, "Component type for IServiceA service in second child scope should have been default.");
                        Assert.IsInstanceOf<ServiceB2>(service3B, "Component type for IServiceB service in second child scope should have been override.");
                    }
                }
            }
        }
    }
}
