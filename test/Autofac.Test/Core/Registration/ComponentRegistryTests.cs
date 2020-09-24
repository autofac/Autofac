// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
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
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            Assert.Throws<ArgumentNullException>(() => registryBuilder.Register(null));
        }

        [Fact]
        public void WhenNoImplementationsRegistered_RegistrationsForServiceIncludeDynamicSources()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.AddRegistrationSource(new ObjectRegistrationSource());
            var registry = registryBuilder.Build();

            Assert.False(registry.Registrations.Where(
                r => r.Services.Contains(new TypedService(typeof(object)))).Any());
            Assert.Single(registry.RegistrationsFor(new TypedService(typeof(object))));
        }

        [Fact]
        public void WhenRegistrationIsMade_ComponentRegisteredEventFired()
        {
            object eventSender = null;
            ComponentRegisteredEventArgs args = null;
            var eventCount = 0;

            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.Registered += (sender, e) =>
            {
                eventSender = sender;
                args = e;
                ++eventCount;
            };

            var registration = Factory.CreateSingletonObjectRegistration();
            registryBuilder.Register(registration);

            Assert.Equal(1, eventCount);
            Assert.NotNull(eventSender);
            Assert.Same(registryBuilder, eventSender);
            Assert.NotNull(args);
            Assert.Same(registryBuilder, args.ComponentRegistryBuilder);
            Assert.Same(registration, args.ComponentRegistration);
        }

        [Fact]
        public void WhenMultipleProvidersOfServiceExist_DefaultRegistrationIsMostRecent()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();

            registryBuilder.Register(r1);
            registryBuilder.Register(r2);

            var registry = registryBuilder.Build();

            Assert.True(registry.TryGetRegistration(new TypedService(typeof(object)), out IComponentRegistration defaultRegistration));
            Assert.Same(r2, defaultRegistration);
        }

        [Fact]
        public void WhenNoImplementers_TryGetRegistrationReturnsFalse()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            var registry = registryBuilder.Build();
            Assert.False(registry.TryGetRegistration(new TypedService(typeof(object)), out _));
        }

        [Fact]
        public void WhenNoImplementerIsDirectlyRegistered_RegistrationCanBeProvidedDynamically()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.AddRegistrationSource(new ObjectRegistrationSource());
            var registry = registryBuilder.Build();
            Assert.True(registry.TryGetRegistration(new TypedService(typeof(object)), out _));
        }

        [Fact]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_ExplicitRegistrationIsDefault()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.Register(r);
            registryBuilder.AddRegistrationSource(new ObjectRegistrationSource());
            var registry = registryBuilder.Build();

            registry.TryGetRegistration(new TypedService(typeof(object)), out IComponentRegistration defaultForObject);

            Assert.Same(r, defaultForObject);
        }

        [Fact]
        public void WhenRegistrationProvidedExplicitlyAndThroughRegistrationSource_BothAreReturnedFromRegistrationsFor()
        {
            var r = Factory.CreateSingletonObjectRegistration();

            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.Register(r);
            registryBuilder.AddRegistrationSource(new ObjectRegistrationSource());
            var registry = registryBuilder.Build();

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

            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.AddRegistrationSource(new ObjectRegistrationSource());
            registryBuilder.Register(r);
            var registry = registryBuilder.Build();

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
                Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
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
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.AddRegistrationSource(new RecursiveRegistrationSource());
            var registry = registryBuilder.Build();

            Assert.False(registry.IsRegistered(new UniqueService()));
        }

        [Fact]
        public void WhenRegistrationsAddedBeforeAndAfterSource_BothAreSeenBySource()
        {
            var r1 = Factory.CreateSingletonObjectRegistration();
            var r2 = Factory.CreateSingletonObjectRegistration();

            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.Register(r1);
            registryBuilder.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            registryBuilder.Register(r2);
            var registry = registryBuilder.Build();

            var wrappedObjects = registry.RegistrationsFor(new TypedService(typeof(Func<object>)));

            Assert.Equal(2, wrappedObjects.Count());
        }

        [Fact]
        public void LastRegistrationSourceRegisteredIsTheDefault()
        {
            var first = new object();
            var second = new object();
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();

            registryBuilder.AddRegistrationSource(new ObjectRegistrationSource(first));
            registryBuilder.AddRegistrationSource(new ObjectRegistrationSource(second));
            var registry = registryBuilder.Build();

            registry.TryGetRegistration(new TypedService(typeof(object)), out IComponentRegistration def);

            var invoker = def.Activator.GetPipelineInvoker(registry);

            var result = invoker(new ContainerBuilder().Build(), Enumerable.Empty<Parameter>());

            Assert.Equal(result, second);
        }

        [Fact]
        public void AfterResolvingAdapter_AddingMoreAdaptees_AddsMoreAdapters()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.AddRegistrationSource(new MetaRegistrationSource());
            var metaService = new TypedService(typeof(Meta<object>));

            var first = RegistrationBuilder.ForType<object>().CreateRegistration();
            registryBuilder.Register(first);

            using (var container = new Container(registryBuilder.Build()))
            {
                var meta1 = container.ComponentRegistry.RegistrationsFor(metaService);
                Assert.Single(meta1);

                var second = RegistrationBuilder.ForType<object>().CreateRegistration();

                using (var lifetimeScope = container.BeginLifetimeScope(builder => builder.ComponentRegistryBuilder.Register(second)))
                {
                    var meta2 = lifetimeScope.ComponentRegistry.RegistrationsFor(metaService);

                    Assert.Equal(2, meta2.Count());
                }
            }
        }

        [Fact]
        public void AdaptingAGeneratedServiceYieldsASingleAdapter()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.AddRegistrationSource(new MetaRegistrationSource());
            registryBuilder.AddRegistrationSource(new CollectionRegistrationSource());
            var registry = registryBuilder.Build();

            var metaCollections = registry.RegistrationsFor(
                new TypedService(typeof(Meta<IEnumerable<object>>)));
            Assert.Single(metaCollections);
        }

        [Fact]
        public void AdaptingAnAdapterYieldsASingleAdapter()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.Register(RegistrationBuilder.ForType<object>().CreateRegistration());
            registryBuilder.AddRegistrationSource(new MetaRegistrationSource());
            registryBuilder.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            var registry = registryBuilder.Build();

            var metaCollections = registry.RegistrationsFor(
                new TypedService(typeof(Meta<Func<object>>)));
            Assert.Single(metaCollections);
        }

        [Fact]
        public void AfterResolvingAdapterType_AddingAnAdapter_AddsAdaptingComponents()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.Register(RegistrationBuilder.ForType<object>().CreateRegistration());
            var adapterService = new TypedService(typeof(Func<object>));

            using (var container = new Container(registryBuilder.Build()))
            {
                var pre = container.ComponentRegistry.RegistrationsFor(adapterService);
                Assert.Empty(pre);

                using (var lifetimeScope = container.BeginLifetimeScope(
                    builder => builder.ComponentRegistryBuilder.AddRegistrationSource(new GeneratedFactoryRegistrationSource())))
                {
                    var post = lifetimeScope.ComponentRegistry.RegistrationsFor(adapterService);
                    Assert.Single(post);
                }
            }
        }

        [Fact]
        public void AddingConcreteImplementationWhenAdapterImplementationsExist_AddsChainedAdapters()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            registryBuilder.AddRegistrationSource(new MetaRegistrationSource());
            registryBuilder.Register(RegistrationBuilder.ForType<object>().CreateRegistration());

            var chainedService = new TypedService(typeof(Meta<Func<object>>));

            using (var container = new Container(registryBuilder.Build()))
            {
                var pre = container.ComponentRegistry.RegistrationsFor(chainedService);
                Assert.Single(pre);

                Func<object> func = () => new object();
                using (var lifetimeScope = container.BeginLifetimeScope(builder =>
                    builder.ComponentRegistryBuilder.Register(RegistrationBuilder.ForDelegate((c, p) => func).CreateRegistration())))
                {
                    var post = lifetimeScope.ComponentRegistry.RegistrationsFor(chainedService);
                    Assert.Equal(2, post.Count());
                }
            }
        }

        [Fact]
        public void WhenAdaptersAreAppliedButNoRegistrationsCreated_AddingAdapteesAddsAdapters()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();
            registryBuilder.AddRegistrationSource(new GeneratedFactoryRegistrationSource());
            var adapterService = new TypedService(typeof(Func<object>));

            using (var container = new Container(registryBuilder.Build()))
            {
                container.ComponentRegistry.RegistrationsFor(adapterService);

                using (var lifetimeScope = container.BeginLifetimeScope(builder =>
                    builder.ComponentRegistryBuilder.Register(RegistrationBuilder.ForType<object>().CreateRegistration())))
                {
                    var adapters = lifetimeScope.ComponentRegistry.RegistrationsFor(adapterService);
                    Assert.Single(adapters);
                }
            }
        }

        [Fact]
        public void WhenASourceIsAddedToTheRegistry_TheSourceAddedEventIsRaised()
        {
            var registryBuilder = Factory.CreateEmptyComponentRegistryBuilder();

            object sender = null;
            RegistrationSourceAddedEventArgs args = null;

            registryBuilder.RegistrationSourceAdded += (s, e) =>
            {
                sender = s;
                args = e;
            };

            var source = new ObjectRegistrationSource();
            registryBuilder.AddRegistrationSource(source);

            Assert.Same(registryBuilder, sender);
            Assert.Same(registryBuilder, args.ComponentRegistry);
            Assert.Same(source, args.RegistrationSource);
        }
    }
}
