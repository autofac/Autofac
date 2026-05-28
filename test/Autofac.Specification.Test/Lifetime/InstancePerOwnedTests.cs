// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Features.OwnedInstances;

namespace Autofac.Specification.Test.Lifetime;

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
        const string ServiceKey = "ServiceKey";
        cb.RegisterType<MessageHandler>().Keyed<MessageHandler>(ServiceKey);
        cb.RegisterType<ServiceForHandler>().InstancePerOwned<MessageHandler>(ServiceKey);
        var container = cb.Build();
        var owned = container.ResolveKeyed<Owned<MessageHandler>>(ServiceKey);
        Assert.Same(owned.Value.LifetimeScope.Tag, owned.Value.DependentService.LifetimeScope.Tag);
    }

    [Fact]
    public void InstancePerOwnedWithKeyResolvesToOwnedScope_NonGenericMethodSignature()
    {
        var cb = new ContainerBuilder();
        const string ServiceKey = "ServiceKey";
        cb.RegisterType<MessageHandler>().Keyed<MessageHandler>(ServiceKey);
        cb.RegisterType<ServiceForHandler>().InstancePerOwned(ServiceKey, typeof(MessageHandler));
        var container = cb.Build();
        var owned = container.ResolveKeyed<Owned<MessageHandler>>(ServiceKey);
        Assert.Same(owned.Value.LifetimeScope.Tag, owned.Value.DependentService.LifetimeScope.Tag);
    }

    [Fact]
    public void InstancePerOwnedWithoutKeysResolvesForOwnedServicesWithKeys()
    {
        var builder = new ContainerBuilder();
        const string ServiceKeyA = "A";
        const string ServiceKeyB = "B";
        builder.RegisterType<Service>().AsSelf().InstancePerOwned<IRoot>();
        builder.RegisterType<RootA>().Keyed<IRoot>(ServiceKeyA);
        builder.RegisterType<RootB>().Keyed<IRoot>(ServiceKeyB);
        var container = builder.Build();

        var ownedRoot = container.ResolveKeyed<Owned<IRoot>>(ServiceKeyA);
        Assert.NotNull(ownedRoot.Value.Dependency);

        ownedRoot = container.ResolveKeyed<Owned<IRoot>>(ServiceKeyB);
        Assert.NotNull(ownedRoot.Value.Dependency);
    }

    [Fact]
    public void InstancePerOwnedWithMultipleKeysResolvesForOwnedServicesWithMatchingKeys()
    {
        var builder = new ContainerBuilder();
        const string ServiceKeyA = "A";
        const string ServiceKeyB = "B";
        builder.RegisterType<Service>().AsSelf().InstancePerOwned<IRoot>(ServiceKeyA, ServiceKeyB);
        builder.RegisterType<RootA>().Keyed<IRoot>(ServiceKeyA);
        builder.RegisterType<RootB>().Keyed<IRoot>(ServiceKeyB);
        var container = builder.Build();

        var ownedRoot = container.ResolveKeyed<Owned<IRoot>>(ServiceKeyA);
        Assert.NotNull(ownedRoot.Value.Dependency);

        ownedRoot = container.ResolveKeyed<Owned<IRoot>>(ServiceKeyB);
        Assert.NotNull(ownedRoot.Value.Dependency);
    }

    [Fact]
    public void InstancePerOwnedThrowsWhenKeyMissingForOwnedServiceWithKey()
    {
        var builder = new ContainerBuilder();
        const string ServiceKeyA = "A";
        const string ServiceKeyB = "B";
        builder.RegisterType<Service>().AsSelf().InstancePerOwned<IRoot>(ServiceKeyA);
        builder.RegisterType<RootA>().Keyed<IRoot>(ServiceKeyA);
        builder.RegisterType<RootB>().Keyed<IRoot>(ServiceKeyB);
        var container = builder.Build();

        var ownedRoot = container.ResolveKeyed<Owned<IRoot>>(ServiceKeyA);
        Assert.NotNull(ownedRoot.Value.Dependency);

        void Resolve() => container.ResolveKeyed<Owned<IRoot>>(ServiceKeyB);
        Assert.Throws<DependencyResolutionException>(Resolve);
    }

    [Fact]
    public void InstancePerOwnedWithKeyThrowsWhenOwnedServiceHasNoKey()
    {
        var builder = new ContainerBuilder();
        const string ServiceKey = "A";
        builder.RegisterType<Service>().AsSelf().InstancePerOwned<IRoot>(ServiceKey);
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

        public ServiceForHandler DependentService
        {
            get; set;
        }

        public ILifetimeScope LifetimeScope
        {
            get; set;
        }
    }

    private class ServiceForHandler
    {
        public ServiceForHandler(ILifetimeScope lifetimeScope)
        {
            LifetimeScope = lifetimeScope;
        }

        public ILifetimeScope LifetimeScope
        {
            get; set;
        }
    }

    private class Service
    {
    }

    private interface IRoot
    {
        Service Dependency
        {
            get;
        }
    }

    private class RootA : IRoot
    {
        public RootA(Service dependency)
        {
            Dependency = dependency;
        }

        public Service Dependency
        {
            get;
        }
    }

    private class RootB : IRoot
    {
        public RootB(Service dependency)
        {
            Dependency = dependency;
        }

        public Service Dependency
        {
            get;
        }
    }
}
