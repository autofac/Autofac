using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    static class AssertionHelpers
    {
        public static void AssertRegistered<TService>(this IComponentContext context)
        {
            Assert.IsTrue(context.IsRegistered<TService>());
        }

        public static void AssertNotRegistered<TService>(this IComponentContext context)
        {
            Assert.IsFalse(context.IsRegistered<TService>());
        }

        public static void AssertRegistered<TService>(this IComponentContext context, string service, string message = "Expected component was not registered.")
        {
            Assert.IsTrue(context.IsRegisteredWithName(service, typeof(TService)), message);
        }

        public static void AssertNotRegistered<TService>(this IComponentContext context, string service)
        {
            Assert.IsFalse(context.IsRegisteredWithName(service, typeof(TService)));
        }
    }
}
