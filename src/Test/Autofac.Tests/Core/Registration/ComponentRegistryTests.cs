using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Builder;
using Autofac.Features.GeneratedFactories;
using NUnit.Framework;
using Autofac.Core.Registration;
using Autofac.Core;
using Autofac.Tests.Scenarios.RegistrationSources;

namespace Autofac.Tests.Core.Registration
{
    [TestFixture]
    public class ComponentRegistryTests
    {
        [Test]
        public void Register_DoesNotAcceptNull()
        {
            var registry = new ComponentRegistry();
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                registry.Register(null);
            });
        }

        [Test]
        public void WhenNoImplementationsRegistered_RegistrationsForServiceIncludeDynamicSources()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            Assert.IsFalse(registry.Registrations.Where(
                r => r.Services.Contains(new TypedService(typeof(object)))).Any());
            Assert.AreEqual(1, registry.RegistrationsFor(new TypedService(typeof(object))).Count());
        }

        [Test]
        public void WhenRegistrationIsMad_ComponentRegisteredEventFired()
        {
            object eventSender = null;
            ComponentRegisteredEventArgs args = null;
            var eventCount = 0;

            var registry = new ComponentRegistry();
            registry.Registered += (sender, e) =>
            {
                eventSender = sender;
                args = e;
                ++eventCount;
            };

            var registration = Factory.CreateSingletonObjectRegistration();
            registry.Register(registration);

            Assert.AreEqual(1, eventCount);
            Assert.IsNotNull(eventSender);
            Assert.AreSame(registry, eventSender);
            Assert.IsNotNull(args);
            Assert.AreSame(registry, args.ComponentRegistry);
            Assert.AreSame(registration, args.ComponentRegistration);
        }

        [Test]
        public void WhenMultipleProvidersOfServiceExist_DefaultRegistrationIsMostRecent()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();

            registry.Register(r1);
            registry.Register(r2);

            IComponentRegistration defaultRegistration;
            Assert.IsTrue(registry.TryGetRegistration(new TypedService(typeof(object)), out defaultRegistration));
            Assert.AreSame(r2, defaultRegistration);
        }

        [Test]
        public void WhenNoImplementers_TryGetRegistrationReturnsFalse()
        {
            var registry = new ComponentRegistry();
            IComponentRegistration unused;
            Assert.IsFalse(registry.TryGetRegistration(new TypedService(typeof(object)), out unused));
        }

        [Test]
        public void WhenNoImplementerIsDirectlyRegistered_RegistrationCanBeProvidedDynamically()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            IComponentRegistration registration;
            Assert.IsTrue(registry.TryGetRegistration(new TypedService(typeof(object)), out registration));
        }

        [Test]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_ExplicitRegistrationIsDefault()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.Register(r);
            registry.AddRegistrationSource(new ObjectRegistrationSource());

            IComponentRegistration defaultForObject = null;
            registry.TryGetRegistration(new TypedService(typeof(object)), out defaultForObject);

            Assert.AreSame(r, defaultForObject);
        }

        [Test]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_BothAreReturnedFromRegistrationsFor()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.Register(r);
            registry.AddRegistrationSource(new ObjectRegistrationSource());

            var forObject = registry.RegistrationsFor(new TypedService(typeof(object)));

            Assert.AreEqual(2, forObject.Count());

            // Just paranoia - make sure we don't regenerate
            forObject = registry.RegistrationsFor(new TypedService(typeof(object)));

            Assert.AreEqual(2, forObject.Count());
        }

        [Test]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_Reordered_BothAreReturnedFromRegistrationsFor()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            registry.Register(r);

            var forObject = registry.RegistrationsFor(new TypedService(typeof(object)));

            Assert.AreEqual(2, forObject.Count());
        }

        [Test]
        public void WhenRegistrationSourcePreservesOrder_DefaultsForWrappersMatchDefaultsForWrapped()
        {
            object a = new object(), b = new object();
            
            var builder = new ContainerBuilder();
            builder.RegisterInstance(a);
            builder.RegisterInstance(b).PreserveExistingDefaults();
            var container = builder.Build();

            Assert.AreSame(a, container.Resolve<object>());
            Assert.AreSame(a, container.Resolve<Func<object>>().Invoke());

            var allObjects = container.Resolve<IEnumerable<object>>();
            Assert.AreEqual(2, allObjects.Count());
            Assert.That(allObjects.Contains(a));
            Assert.That(allObjects.Contains(b));

            var allFuncs = container.Resolve<IEnumerable<Func<object>>>();
            Assert.AreEqual(2, allFuncs.Count());
            Assert.That(allFuncs.Any(f => f() == a));
            Assert.That(allFuncs.Any(f => f() == b));
        }

        class RecursiveRegistrationSource : IRegistrationSource
        {
            public IEnumerable<IComponentRegistration> RegistrationsFor(
                Service service, 
                Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
            {
                registrationAccessor(service);
                return Enumerable.Empty<IComponentRegistration>();
            }
        }

        [Test]
        public void WhenARegistrationSourceQueriesForTheSameService_ItIsNotRecursivelyQueried()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new RecursiveRegistrationSource());
            Assert.False(registry.IsRegistered(new UniqueService()));
        }

        [Test]
        public void WhenRegistrationsAddedBeforeAndAfterSource_BothAreSeenBySource()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.Register(r1);
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            registry.Register(r2);

            var wrappedObjects = registry.RegistrationsFor(new TypedService(typeof(Func<object>)));

            Assert.AreEqual(2, wrappedObjects.Count());
        }

        [Test]
        public void LastRegistrationSourceRegisteredIsTheDefault()
        {
            var first = new object();
            var second = new object();
            var registry = new ComponentRegistry();

            registry.AddRegistrationSource(new ObjectRegistrationSource(first));
            registry.AddRegistrationSource(new ObjectRegistrationSource(second));

            IComponentRegistration def;
            registry.TryGetRegistration(new TypedService(typeof(object)), out def);

            var result = def.Activator.ActivateInstance(Container.Empty, Enumerable.Empty<Parameter>());

            Assert.AreEqual(result, second);
        }
    }
}
