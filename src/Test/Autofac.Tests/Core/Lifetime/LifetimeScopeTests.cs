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
            builder.Register( c => o1 );
            var scope1 = builder.Build();

            var scope2 = scope1.BeginLifetimeScope( b => b.Register( c => o2 ) );

            // T.S: What's strange is that if you don't specify any new (unrelated) registrations
            // in the local (3rd) scope, the test passes.
            var scope3 = scope2.BeginLifetimeScope( b => b.Register( c => "s3" ) ); // <- Fails with this.

            Assert.AreSame( o2, scope3.Resolve<object>() );
        }

        [Test]
        [Ignore("Limitation")]
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

        public interface IParty
        {
            string Name { get; set; }
        }

        public class Person : IParty
        {
            public string Name { get; set; }
        }

        public class AddressBook
        {
            private readonly IList<IParty> _contacts = new List<IParty>();
            private readonly Func<IParty> _partyFactory;

            public AddressBook(Func<IParty> partyFactory)
            {
                _partyFactory = partyFactory;
            }

            public IParty AddNew(string name)
            {
                IParty newContact = _partyFactory();
                newContact.Name = name;

                _contacts.Add(newContact);

                return newContact;
            }
        }

        [Test]
        // Previously threw ComponentNotRegisteredException
        public void MultiLayerNestingScenario_FromTysonStolarski()
        {
            var builder = new ContainerBuilder();
            var level1Scope = builder.Build();

            var level2Scope = level1Scope.BeginLifetimeScope(cb =>
            {
                cb.RegisterType<Person>().As<IParty>().InstancePerDependency();
                cb.RegisterType<AddressBook>().InstancePerDependency();
            });

            var level3Scope = level2Scope.BeginLifetimeScope(cb =>
                cb.Register(c => new Person()).As<IParty>().InstancePerDependency()
            );

            var addressBook = level3Scope.Resolve<AddressBook>();
            addressBook.AddNew("Robert");
        }

        [Test]
        public void InstancesRegisteredInNestedScopeAreSingletonsInThatScope()
        {
            var builder = new ContainerBuilder();
            var rootScope = builder.Build();

            var dt = new DisposeTracker();

            var nestedScope = rootScope.BeginLifetimeScope(cb =>
                 cb.RegisterInstance(dt));

            var dt1 = nestedScope.Resolve<DisposeTracker>();
            Assert.AreSame(dt, dt1);
        }


        [Test]
        public void SingletonsRegisteredInNestedScopeAreTiedToThatScope()
        {
            var builder = new ContainerBuilder();
            var rootScope = builder.Build();

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
    }
}
