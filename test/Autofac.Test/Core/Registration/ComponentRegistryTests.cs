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
            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            Assert.Throws<ArgumentNullException>(() => builder.Register(null));
        }

        [Fact]
        public void WhenNoImplementationsRegistered_RegistrationsForServiceIncludeDynamicSources()
        {
            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            builder.AddRegistrationSource(new ObjectRegistrationSource());
            var registry = builder.Build();

            Assert.DoesNotContain(registry.Registrations, r => r.Services.Contains(new TypedService(typeof(object))));
            Assert.Single(registry.RegistrationsFor(new TypedService(typeof(object))));
        }

        [Fact]
        public void WhenRegistrationIsMade_ComponentRegisteredEventFired()
        {
            object eventSender = null;
            ComponentRegisteredEventArgs args = null;
            var eventCount = 0;

            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            builder.Registered += (sender, e) =>
            {
                eventSender = sender;
                args = e;
                ++eventCount;
            };

            var numberOfRegistrations = eventCount;

            var registration = Factory.CreateSingletonObjectRegistration();
            builder.Register(registration);

            Assert.Equal(numberOfRegistrations + 1, eventCount);
            Assert.NotNull(eventSender);
            Assert.Same(builder, eventSender);
            Assert.NotNull(args);
            Assert.Same(builder, args.ComponentRegistry);
            Assert.Same(registration, args.ComponentRegistration);
        }

        [Fact]
        public void WhenMultipleProvidersOfServiceExist_DefaultRegistrationIsMostRecent()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());

            builder.Register(r1);
            builder.Register(r2);

            var registry = builder.Build();

            IComponentRegistration defaultRegistration;
            Assert.True(registry.TryGetRegistration(new TypedService(typeof(object)), out defaultRegistration));
            Assert.Same(r2, defaultRegistration);
        }

        [Fact]
        public void WhenNoImplementers_TryGetRegistrationReturnsFalse()
        {
            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            var registry = builder.Build();

            IComponentRegistration unused;
            Assert.False(registry.TryGetRegistration(new TypedService(typeof(object)), out unused));
        }

        [Fact]
        public void WhenNoImplementerIsDirectlyRegistered_RegistrationCanBeProvidedDynamically()
        {
            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            builder.AddRegistrationSource(new ObjectRegistrationSource());
            var registry = builder.Build();
            IComponentRegistration registration;
            Assert.True(registry.TryGetRegistration(new TypedService(typeof(object)), out registration));
        }

        [Fact]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_ExplicitRegistrationIsDefault()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            builder.Register(r);
            builder.AddRegistrationSource(new ObjectRegistrationSource());
            var registry = builder.Build();

            IComponentRegistration defaultForObject;
            registry.TryGetRegistration(new TypedService(typeof(object)), out defaultForObject);

            Assert.Same(r, defaultForObject);
        }

        [Fact]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_BothAreReturnedFromRegistrationsFor()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            builder.Register(r);
            builder.AddRegistrationSource(new ObjectRegistrationSource());
            var registry = builder.Build();

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

            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());

            builder.AddRegistrationSource(new ObjectRegistrationSource());
            builder.Register(r);

            var registry = builder.Build();
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
            Assert.Contains(a, allObjects);
            Assert.Contains(b, allObjects);

            var allFuncs = container.Resolve<IEnumerable<Func<object>>>();
            Assert.Equal(2, allFuncs.Count());
            Assert.Contains(allFuncs, f => f() == a);
            Assert.Contains(allFuncs, f => f() == b);
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
            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            builder.AddRegistrationSource(new RecursiveRegistrationSource());
            var registry = builder.Build();
            Assert.False(registry.IsRegistered(new UniqueService()));
        }

        [Fact]
        public void WhenRegistrationsAddedBeforeAndAfterSource_BothAreSeenBySource()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            builder.Register(r1);
            builder.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            builder.Register(r2);
            var registry = builder.Build();

            var wrappedObjects = registry.RegistrationsFor(new TypedService(typeof(Func<object>)));

            Assert.Equal(2, wrappedObjects.Count());
        }

        [Fact]
        public void LastRegistrationSourceRegisteredIsTheDefault()
        {
            var first = new object();
            var second = new object();
            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());

            builder.AddRegistrationSource(new ObjectRegistrationSource(first));
            builder.AddRegistrationSource(new ObjectRegistrationSource(second));

            var registry = builder.Build();
            IComponentRegistration def;
            registry.TryGetRegistration(new TypedService(typeof(object)), out def);

            var result = def.Activator.ActivateInstance(new ContainerBuilder().Build(), Enumerable.Empty<Parameter>());

            Assert.Equal(result, second);
        }

        [Fact]
        public void AdaptingAGeneratedServiceYieldsASingleAdapter()
        {
            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());

            builder.AddRegistrationSource(new MetaRegistrationSource());
            builder.AddRegistrationSource(new CollectionRegistrationSource());
            var registry = builder.Build();
            var metaCollections = registry.RegistrationsFor(new TypedService(typeof(Meta<IEnumerable<object>>)));
            Assert.Single(metaCollections);
        }

        [Fact]
        public void AdaptingAnAdapterYieldsASingleAdapter()
        {
            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());
            builder.Register(RegistrationBuilder.ForType<object>().CreateRegistration());
            builder.AddRegistrationSource(new MetaRegistrationSource());
            builder.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            var registry = builder.Build();
            var metaCollections = registry.RegistrationsFor(
                new TypedService(typeof(Meta<Func<object>>)));
            Assert.Single(metaCollections);
        }

        [Fact]
        public void WhenASourceIsAddedToTheRegistry_TheSourceAddedEventIsRaised()
        {
            IComponentRegistryBuilder builder = new ComponentRegistry(new Dictionary<string, object>());

            object sender = null;
            RegistrationSourceAddedEventArgs args = null;

            builder.RegistrationSourceAdded += (s, e) =>
            {
                sender = s;
                args = e;
            };

            var source = new ObjectRegistrationSource();
            builder.AddRegistrationSource(source);

            Assert.Same(builder, sender);
            Assert.Same(builder, args.ComponentRegistry);
            Assert.Same(source, args.RegistrationSource);
        }
    }
}
