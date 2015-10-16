using Autofac.Core;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    static class Assertions
    {
        public static void AssertRegistered<TService>(this IComponentContext context)
        {
            Assert.True(context.IsRegistered<TService>());
        }

        public static void AssertNotRegistered<TService>(this IComponentContext context)
        {
            Assert.False(context.IsRegistered<TService>());
        }

        public static void AssertImplementation<TService, TImplementation>(this IComponentContext context)
        {
            var service = context.Resolve<TService>();
            Assert.IsAssignableFrom<TImplementation>(service);
        }

        public static void AssertSharing<TComponent>(this IComponentContext context, InstanceSharing sharing)
        {
            var cr = context.RegistrationFor<TComponent>();
            Assert.Equal(sharing, cr.Sharing);
        }

        public static void AssertLifetime<TComponent, TLifetime>(this IComponentContext context)
        {
            var cr = context.RegistrationFor<TComponent>();
            Assert.IsType<TLifetime>(cr.Lifetime);
        }

        public static void AssertOwnership<TComponent>(this IComponentContext context, InstanceOwnership ownership)
        {
            var cr = context.RegistrationFor<TComponent>();
            Assert.Equal(ownership, cr.Ownership);
        }

        public static IComponentRegistration RegistrationFor<TComponent>(this IComponentContext context)
        {
            IComponentRegistration r;
            Assert.True(context.ComponentRegistry.TryGetRegistration(new TypedService(typeof(TComponent)), out r));
            return r;
        }
    }
}
