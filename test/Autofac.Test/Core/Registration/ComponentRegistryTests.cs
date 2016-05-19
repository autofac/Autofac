using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.Collections;
using Autofac.Features.GeneratedFactories;
using Autofac.Features.Metadata;
using Autofac.Test.Scenarios.RegistrationSources;
using Xunit;

namespace Autofac.Test.Core.Registration
{
    public class ComponentRegistryTests
    {
        [Fact]
        public void Register_DoesNotAcceptNull()
        {
            var registry = new ComponentRegistry();
            Assert.Throws<ArgumentNullException>(() => registry.Register(null));
        }

        [Fact]
        public void WhenNoImplementationsRegistered_RegistrationsForServiceIncludeDynamicSources()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            Assert.False(registry.Registrations.Where(
                r => r.Services.Contains(new TypedService(typeof(object)))).Any());
            Assert.Equal(1, registry.RegistrationsFor(new TypedService(typeof(object))).Count());
        }

        [Fact]
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

            Assert.Equal(1, eventCount);
            Assert.NotNull(eventSender);
            Assert.Same(registry, eventSender);
            Assert.NotNull(args);
            Assert.Same(registry, args.ComponentRegistry);
            Assert.Same(registration, args.ComponentRegistration);
        }

        [Fact]
        public void WhenMultipleProvidersOfServiceExist_DefaultRegistrationIsMostRecent()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();

            registry.Register(r1);
            registry.Register(r2);

            IComponentRegistration defaultRegistration;
            Assert.True(registry.TryGetRegistration(new TypedService(typeof(object)), out defaultRegistration));
            Assert.Same(r2, defaultRegistration);
        }

        [Fact]
        public void WhenNoImplementers_TryGetRegistrationReturnsFalse()
        {
            var registry = new ComponentRegistry();
            IComponentRegistration unused;
            Assert.False(registry.TryGetRegistration(new TypedService(typeof(object)), out unused));
        }

        [Fact]
        public void WhenNoImplementerIsDirectlyRegistered_RegistrationCanBeProvidedDynamically()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            IComponentRegistration registration;
            Assert.True(registry.TryGetRegistration(new TypedService(typeof(object)), out registration));
        }

        [Fact]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_ExplicitRegistrationIsDefault()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.Register(r);
            registry.AddRegistrationSource(new ObjectRegistrationSource());

            IComponentRegistration defaultForObject;
            registry.TryGetRegistration(new TypedService(typeof(object)), out defaultForObject);

            Assert.Same(r, defaultForObject);
        }

        [Fact]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_BothAreReturnedFromRegistrationsFor()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.Register(r);
            registry.AddRegistrationSource(new ObjectRegistrationSource());

            var forObject = registry.RegistrationsFor(new TypedService(typeof(object)));

            Assert.Equal(2, forObject.Count());

            // Just paranoia - make sure we don't regenerate
            forObject = registry.RegistrationsFor(new TypedService(typeof(object)));

            Assert.Equal(2, forObject.Count());
        }

        [Fact]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_Reordered_BothAreReturnedFromRegistrationsFor()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new ObjectRegistrationSource());
            registry.Register(r);

            var forObject = registry.RegistrationsFor(new TypedService(typeof(object)));

            Assert.Equal(2, forObject.Count());
        }

        [Fact]
        public void WhenRegistrationSourcePreservesOrder_DefaultsForWrappersMatchDefaultsForWrapped()
        {
            object a = new object(), b = new object();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(a);
            builder.RegisterInstance(b).PreserveExistingDefaults();
            var container = builder.Build();

            Assert.Same(a, container.Resolve<object>());
            Assert.Same(a, container.Resolve<Func<object>>().Invoke());

            var allObjects = container.Resolve<IEnumerable<object>>();
            Assert.Equal(2, allObjects.Count());
            Assert.True(allObjects.Contains(a));
            Assert.True(allObjects.Contains(b));

            var allFuncs = container.Resolve<IEnumerable<Func<object>>>();
            Assert.Equal(2, allFuncs.Count());
            Assert.True(allFuncs.Any(f => f() == a));
            Assert.True(allFuncs.Any(f => f() == b));
        }

        internal class RecursiveRegistrationSource : IRegistrationSource
        {
            public IEnumerable<IComponentRegistration> RegistrationsFor(
                Service service,
                Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
            {
                registrationAccessor(service);
                return Enumerable.Empty<IComponentRegistration>();
            }

            public bool IsAdapterForIndividualComponents
            {
                get { return false; }
            }
        }

        [Fact]
        public void WhenARegistrationSourceQueriesForTheSameService_ItIsNotRecursivelyQueried()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new RecursiveRegistrationSource());
            Assert.False(registry.IsRegistered(new UniqueService()));
        }

        [Fact]
        public void WhenRegistrationsAddedBeforeAndAfterSource_BothAreSeenBySource()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            var registry = new ComponentRegistry();
            registry.Register(r1);
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            registry.Register(r2);

            var wrappedObjects = registry.RegistrationsFor(new TypedService(typeof(Func<object>)));

            Assert.Equal(2, wrappedObjects.Count());
        }

        [Fact]
        public void LastRegistrationSourceRegisteredIsTheDefault()
        {
            var first = new object();
            var second = new object();
            var registry = new ComponentRegistry();

            registry.AddRegistrationSource(new ObjectRegistrationSource(first));
            registry.AddRegistrationSource(new ObjectRegistrationSource(second));

            IComponentRegistration def;
            registry.TryGetRegistration(new TypedService(typeof(object)), out def);

            var result = def.Activator.ActivateInstance(new ContainerBuilder().Build(), Enumerable.Empty<Parameter>());

            Assert.Equal(result, second);
        }

        [Fact]
        public void AfterResolvingAdapter_AddingMoreAdaptees_AddsMoreAdapters()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new MetaRegistrationSource());
            var metaService = new TypedService(typeof(Meta<object>));

            var first = RegistrationBuilder.ForType<object>().CreateRegistration();
            registry.Register(first);

            var meta1 = registry.RegistrationsFor(metaService);
            var firstMeta = meta1.First();

            var second = RegistrationBuilder.ForType<object>().CreateRegistration();
            registry.Register(second);

            var meta2 = registry.RegistrationsFor(metaService);

            Assert.Equal(2, meta2.Count());
            Assert.True(meta2.Contains(firstMeta));
            Assert.Equal(new[] { first, second }, meta2.Select(m => m.Target));
        }

        [Fact]
        public void AdaptingAGeneratedServiceYieldsASingleAdapter()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new MetaRegistrationSource());
            registry.AddRegistrationSource(new CollectionRegistrationSource());
            var metaCollections = registry.RegistrationsFor(
                new TypedService(typeof(Meta<IEnumerable<object>>)));
            Assert.Equal(1, metaCollections.Count());
        }

        [Fact]
        public void AdaptingAnAdapterYieldsASingleAdapter()
        {
            var registry = new ComponentRegistry();
            registry.Register(RegistrationBuilder.ForType<object>().CreateRegistration());
            registry.AddRegistrationSource(new MetaRegistrationSource());
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            var metaCollections = registry.RegistrationsFor(
                new TypedService(typeof(Meta<Func<object>>)));
            Assert.Equal(1, metaCollections.Count());
        }

        [Fact]
        public void AfterResolvingAdapterType_AddingAnAdapter_AddsAdaptingComponents()
        {
            var registry = new ComponentRegistry();
            registry.Register(RegistrationBuilder.ForType<object>().CreateRegistration());
            var adapterService = new TypedService(typeof(Func<object>));
            var pre = registry.RegistrationsFor(adapterService);
            Assert.Equal(0, pre.Count());
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            var post = registry.RegistrationsFor(adapterService);
            Assert.Equal(1, post.Count());
        }

        [Fact]
        public void AddingConcreteImplementationWhenAdapterImplementationsExist_AddsChainedAdapters()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            registry.AddRegistrationSource(new MetaRegistrationSource());
            registry.Register(RegistrationBuilder.ForType<object>().CreateRegistration());

            var chainedService = new TypedService(typeof(Meta<Func<object>>));

            var pre = registry.RegistrationsFor(chainedService);
            Assert.Equal(1, pre.Count());

            Func<object> func = () => new object();
            registry.Register(RegistrationBuilder.ForDelegate((c, p) => func).CreateRegistration());

            var post = registry.RegistrationsFor(chainedService);
            Assert.Equal(2, post.Count());
        }

        [Fact]
        public void WhenAdaptersAreAppliedButNoRegistrationsCreated_AddingAdapteesAddsAdapters()
        {
            var registry = new ComponentRegistry();
            registry.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            var adapterService = new TypedService(typeof(Func<object>));
            registry.RegistrationsFor(adapterService);
            registry.Register(RegistrationBuilder.ForType<object>().CreateRegistration());
            var adapters = registry.RegistrationsFor(adapterService);
            Assert.Equal(1, adapters.Count());
        }

        [Fact]
        public void WhenASourceIsAddedToTheRegistry_TheSourceAddedEventIsRaised()
        {
            var registry = new ComponentRegistry();

            object sender = null;
            RegistrationSourceAddedEventArgs args = null;

            registry.RegistrationSourceAdded += (s, e) =>
            {
                sender = s;
                args = e;
            };

            var source = new ObjectRegistrationSource();
            registry.AddRegistrationSource(source);

            Assert.Same(registry, sender);
            Assert.Same(registry, args.ComponentRegistry);
            Assert.Same(source, args.RegistrationSource);
        }
    }
}
