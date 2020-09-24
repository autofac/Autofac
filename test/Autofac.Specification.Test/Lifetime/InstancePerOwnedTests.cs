// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Features.OwnedInstances;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class InstancePerOwnedTests
    {
        [Fact]
        public void InstancePerOwnedRequiresOwner()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<MessageHandler>();
            cb.RegisterType<ServiceForHandler>().InstancePerOwned<MessageHandler>();
            var container = cb.Build();
            Assert.Throws<DependencyResolutionException>(() => container.Resolve<ServiceForHandler>());
        }

        [Fact]
        public void InstancePerOwnedResolvesToOwnedScope_GenericMethodSignature()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<MessageHandler>();
            cb.RegisterType<ServiceForHandler>().InstancePerOwned<MessageHandler>();
            var container = cb.Build();
            var owned = container.Resolve<Owned<MessageHandler>>();
            Assert.Same(owned.Value.LifetimeScope.Tag, owned.Value.DependentService.LifetimeScope.Tag);
        }

        [Fact]
        public void InstancePerOwnedResolvesToOwnedScope_NonGenericMethodSignature()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<MessageHandler>();
            cb.RegisterType<ServiceForHandler>().InstancePerOwned(typeof(MessageHandler));
            var container = cb.Build();
            var owned = container.Resolve<Owned<MessageHandler>>();
            Assert.Same(owned.Value.LifetimeScope.Tag, owned.Value.DependentService.LifetimeScope.Tag);
        }

        [Fact]
        public void InstancePerOwnedWithKeyResolvesToOwnedScope_GenericMethodSignature()
        {
            var cb = new ContainerBuilder();
            const string serviceKey = "ServiceKey";
            cb.RegisterType<MessageHandler>().Keyed<MessageHandler>(serviceKey);
            cb.RegisterType<ServiceForHandler>().InstancePerOwned<MessageHandler>(serviceKey);
            var container = cb.Build();
            var owned = container.ResolveKeyed<Owned<MessageHandler>>(serviceKey);
            Assert.Same(owned.Value.LifetimeScope.Tag, owned.Value.DependentService.LifetimeScope.Tag);
        }

        [Fact]
        public void InstancePerOwnedWithKeyResolvesToOwnedScope_NonGenericMethodSignature()
        {
            var cb = new ContainerBuilder();
            const string serviceKey = "ServiceKey";
            cb.RegisterType<MessageHandler>().Keyed<MessageHandler>(serviceKey);
            cb.RegisterType<ServiceForHandler>().InstancePerOwned(serviceKey, typeof(MessageHandler));
            var container = cb.Build();
            var owned = container.ResolveKeyed<Owned<MessageHandler>>(serviceKey);
            Assert.Same(owned.Value.LifetimeScope.Tag, owned.Value.DependentService.LifetimeScope.Tag);
        }

        [Fact]
        public void InstancePerOwnedWithoutKeysResolvesForOwnedServicesWithKeys()
        {
            var builder = new ContainerBuilder();
            const string serviceKeyA = "A";
            const string serviceKeyB = "B";
            builder.RegisterType<Service>().AsSelf().InstancePerOwned<IRoot>();
            builder.RegisterType<RootA>().Keyed<IRoot>(serviceKeyA);
            builder.RegisterType<RootB>().Keyed<IRoot>(serviceKeyB);
            var container = builder.Build();

            var ownedRoot = container.ResolveKeyed<Owned<IRoot>>(serviceKeyA);
            Assert.NotNull(ownedRoot.Value.Dependency);

            ownedRoot = container.ResolveKeyed<Owned<IRoot>>(serviceKeyB);
            Assert.NotNull(ownedRoot.Value.Dependency);
        }

        [Fact]
        public void InstancePerOwnedWithMultipleKeysResolvesForOwnedServicesWithMatchingKeys()
        {
            var builder = new ContainerBuilder();
            const string serviceKeyA = "A";
            const string serviceKeyB = "B";
            builder.RegisterType<Service>().AsSelf().InstancePerOwned<IRoot>(serviceKeyA, serviceKeyB);
            builder.RegisterType<RootA>().Keyed<IRoot>(serviceKeyA);
            builder.RegisterType<RootB>().Keyed<IRoot>(serviceKeyB);
            var container = builder.Build();

            var ownedRoot = container.ResolveKeyed<Owned<IRoot>>(serviceKeyA);
            Assert.NotNull(ownedRoot.Value.Dependency);

            ownedRoot = container.ResolveKeyed<Owned<IRoot>>(serviceKeyB);
            Assert.NotNull(ownedRoot.Value.Dependency);
        }

        [Fact]
        public void InstancePerOwnedThrowsWhenKeyMissingForOwnedServiceWithKey()
        {
            var builder = new ContainerBuilder();
            const string serviceKeyA = "A";
            const string serviceKeyB = "B";
            builder.RegisterType<Service>().AsSelf().InstancePerOwned<IRoot>(serviceKeyA);
            builder.RegisterType<RootA>().Keyed<IRoot>(serviceKeyA);
            builder.RegisterType<RootB>().Keyed<IRoot>(serviceKeyB);
            var container = builder.Build();

            var ownedRoot = container.ResolveKeyed<Owned<IRoot>>(serviceKeyA);
            Assert.NotNull(ownedRoot.Value.Dependency);

            void Resolve() => container.ResolveKeyed<Owned<IRoot>>(serviceKeyB);
            Assert.Throws<DependencyResolutionException>(Resolve);
        }

        [Fact]
        public void InstancePerOwnedWithKeyThrowsWhenOwnedServiceHasNoKey()
        {
            var builder = new ContainerBuilder();
            const string serviceKey = "A";
            builder.RegisterType<Service>().AsSelf().InstancePerOwned<IRoot>(serviceKey);
            builder.RegisterType<RootA>().As<IRoot>();
            var container = builder.Build();

            void Resolve() => container.Resolve<Owned<IRoot>>();
            Assert.Throws<DependencyResolutionException>(Resolve);
        }

        private class MessageHandler
        {
            public MessageHandler(ILifetimeScope lifetimeScope, ServiceForHandler service)
            {
                DependentService = service;
                LifetimeScope = lifetimeScope;
            }

            public ServiceForHandler DependentService { get; set; }

            public ILifetimeScope LifetimeScope { get; set; }
        }

        private class ServiceForHandler
        {
            public ServiceForHandler(ILifetimeScope lifetimeScope)
            {
                LifetimeScope = lifetimeScope;
            }

            public ILifetimeScope LifetimeScope { get; set; }
        }

        public class Service
        {
        }

        public interface IRoot
        {
            Service Dependency { get; }
        }

        public class RootA : IRoot
        {
            public RootA(Service dependency)
            {
                Dependency = dependency;
            }

            public Service Dependency { get; }
        }

        public class RootB : IRoot
        {
            public RootB(Service dependency)
            {
                Dependency = dependency;
            }

            public Service Dependency { get; }
        }
    }
}
