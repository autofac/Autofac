using Autofac.Core;
using Autofac.Features.OwnedInstances;
using Xunit;

namespace Autofac.Test.Builder
{
    public class InstancePerOwnedRegistrationBuilderTests
    {
        public class MessageHandler
        {
            public ILifetimeScope LifetimeScope { get; set; }

            public ServiceForHandler DependentService { get; set; }

            public MessageHandler(ILifetimeScope lifetimeScope, ServiceForHandler service)
            {
                DependentService = service;
                LifetimeScope = lifetimeScope;
            }
        }

        public class ServiceForHandler
        {
            public ILifetimeScope LifetimeScope { get; set; }

            public ServiceForHandler(ILifetimeScope lifetimeScope)
            {
                LifetimeScope = lifetimeScope;
            }
        }

        [Fact]
        public void InstancePerOwnedResolvesToOwnedScope_GenericMethodSignature()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<MessageHandler>();
            cb.RegisterType<ServiceForHandler>().InstancePerOwned<MessageHandler>();
            var container = cb.Build();

            var owned = container.Resolve<Owned<MessageHandler>>();

            AssertInstancePerOwnedResolvesToOwnedScope(owned, new TypedService(typeof(MessageHandler)));
        }

        [Fact]
        public void InstancePerOwnedResolvesToOwnedScope_NonGenericMethodSignature()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<MessageHandler>();
            cb.RegisterType<ServiceForHandler>().InstancePerOwned(typeof(MessageHandler));
            var container = cb.Build();

            var owned = container.Resolve<Owned<MessageHandler>>();

            AssertInstancePerOwnedResolvesToOwnedScope(owned, new TypedService(typeof(MessageHandler)));
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

            AssertInstancePerOwnedResolvesToOwnedScope(owned, new KeyedService(serviceKey, typeof(MessageHandler)));
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

            AssertInstancePerOwnedResolvesToOwnedScope(owned, new KeyedService(serviceKey, typeof(MessageHandler)));
        }

        private static void AssertInstancePerOwnedResolvesToOwnedScope(Owned<MessageHandler> owned, IServiceWithType tag)
        {
            var handler = owned.Value;
            var dependentService = handler.DependentService;

            Assert.Equal(tag, handler.LifetimeScope.Tag);
            Assert.Same(handler.LifetimeScope, dependentService.LifetimeScope);
        }
    }
}
