using System;
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
        [Ignore("Issue #272")]
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
    }
}
