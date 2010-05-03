using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Features.Collections;
using Autofac.Features.GeneratedFactories;
using NUnit.Framework;
using Autofac.Core.Registration;
using Autofac.Core;
using Autofac.Tests.Scenarios.RegistrationSources;
using Autofac.Features.Metadata;

namespace Autofac.Tests.Core.Registration
{
    [TestFixture]
    public class ComponentRegistryTests
    {
        [Test]
        public void Register_DoesNotAcceptNull()
        {
            var registry = new ComponentRegistry();
            Assertions.AssertThrows<ArgumentNullException>(() => registry.Register(null));
        }

        [Test]
        public void WhenNoImplementationsRegistered_RegistrationsForServiceIncludeDynamicSources()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource(), false);
            Assert.IsFalse(registry.Registrations.Where(
                r => r.Services.Contains(new TypedService(typeof(object)))).Any());
            Assert.AreEqual(1, registry.RegistrationsFor(new TypedService(typeof(object))).Count());
        }

        [Test]
        public void WhenRegistrationIsMade_ComponentRegisteredEventFired()
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
            registry.AddRegistrationSource(new ObjectRegistrationSource(), false);
            IComponentRegistration registration;
            Assert.IsTrue(registry.TryGetRegistration(new TypedService(typeof(object)), out registration));
        }

        [Test]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_ExplicitRegistrationIsDefault()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.Register(r);
            registry.AddRegistrationSource(new ObjectRegistrationSource(), false);

            IComponentRegistration defaultForObject;
            registry.TryGetRegistration(new TypedService(typeof(object)), out defaultForObject);

            Assert.AreSame(r, defaultForObject);
        }

        [Test]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_BothAreReturnedFromRegistrationsFor()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.Register(r);
            registry.AddRegistrationSource(new ObjectRegistrationSource(), false);

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
            registry.AddRegistrationSource(new ObjectRegistrationSource(), false);
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
            registry.AddRegistrationSource(new RecursiveRegistrationSource(), false);
            Assert.False(registry.IsRegistered(new UniqueService()));
        }

        [Test]
        public void WhenRegistrationsAddedBeforeAndAfterSource_BothAreSeenBySource()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.Register(r1);
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource(), true);
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

            registry.AddRegistrationSource(new ObjectRegistrationSource(first), false);
            registry.AddRegistrationSource(new ObjectRegistrationSource(second), false);

            IComponentRegistration def;
            registry.TryGetRegistration(new TypedService(typeof(object)), out def);

            var result = def.Activator.ActivateInstance(Container.Empty, Enumerable.Empty<Parameter>());

            Assert.AreEqual(result, second);
        }

        [Test]
        public void AfterResolvingAdapter_AddingMoreAdaptees_AddsMoreAdapters()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new MetaRegistrationSource(), true);
            var metaService = new TypedService(typeof(Meta<object>));

            var first = RegistrationBuilder.ForType<object>().CreateRegistration();
            registry.Register(first);

            var meta1 = registry.RegistrationsFor(metaService);
            var firstMeta = meta1.First();

            var second = RegistrationBuilder.ForType<object>().CreateRegistration();
            registry.Register(second);

            var meta2 = registry.RegistrationsFor(metaService);

            Assert.That(meta2.Count(), Is.EqualTo(2));
            Assert.That(meta2.Contains(firstMeta));
            Assert.That(meta2.Select(m => m.Target), Is.EquivalentTo(new[] { first, second }));
        }

        [Test]
        public void AdaptingAGeneratedServiceYieldsASingleAdapter()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new MetaRegistrationSource(), true);
            registry.AddRegistrationSource(new CollectionRegistrationSource(), false);
            var metaCollections = registry.RegistrationsFor(
                new TypedService(typeof(Meta<IEnumerable<object>>)));
            Assert.AreEqual(1, metaCollections.Count());
        }

        [Test]
        public void AdaptingAnAdapterYieldsASingleAdapter()
        {
            var registry = new ComponentRegistry();
            registry.Register(RegistrationBuilder.ForType<object>().CreateRegistration());
            registry.AddRegistrationSource(new MetaRegistrationSource(), true);
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource(), true);
            var metaCollections = registry.RegistrationsFor(
                new TypedService(typeof(Meta<Func<object>>)));
            Assert.AreEqual(1, metaCollections.Count());
        }

        [Test]
        public void AfterResolvingAdapterType_AddingAnAdapter_AddsAdaptingComponents()
        {
            var registry = new ComponentRegistry();
            registry.Register(RegistrationBuilder.ForType<object>().CreateRegistration());
            var adapterService = new TypedService(typeof(Func<object>));
            var pre = registry.RegistrationsFor(adapterService);
            Assert.AreEqual(0, pre.Count());
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource(), true);
            var post = registry.RegistrationsFor(adapterService);
            Assert.AreEqual(1, post.Count());
        }

        [Test, Ignore("Limitation")]
        public void AddingConcreteImplementationWhenAdapterImplementationsExist_AddsChainedAdapters()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource(), true);
            registry.AddRegistrationSource(new MetaRegistrationSource(), true);
            registry.Register(RegistrationBuilder.ForType<object>().CreateRegistration());

            var chainedService = new TypedService(typeof(Meta<Func<object>>));

            var pre = registry.RegistrationsFor(chainedService);
            Assert.AreEqual(1, pre.Count());

            Func<object> func = () => new object();
            registry.Register(RegistrationBuilder.ForDelegate((c, p) => func).CreateRegistration());

            var post = registry.RegistrationsFor(chainedService);
            Assert.AreEqual(2, pre.Count());
        }
    }
}
