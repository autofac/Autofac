using System;
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

        private class MessageHandler
        {
            public MessageHandler(ILifetimeScope lifetimeScope, ServiceForHandler service)
            {
                this.DependentService = service;
                this.LifetimeScope = lifetimeScope;
            }

            public ServiceForHandler DependentService { get; set; }

            public ILifetimeScope LifetimeScope { get; set; }
        }

        private class ServiceForHandler
        {
            public ServiceForHandler(ILifetimeScope lifetimeScope)
            {
                this.LifetimeScope = lifetimeScope;
            }

            public ILifetimeScope LifetimeScope { get; set; }
        }
    }
}
