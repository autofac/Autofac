using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    static class AssertionHelpers
    {
        public static void AssertRegistered<TService>(this IComponentContext context, string message = "Expected component was not registered.")
        {
            Assert.IsTrue(context.IsRegistered<TService>());
        }

        public static void AssertNotRegistered<TService>(this IComponentContext context, string message = "Component was registered unexpectedly.")
        {
            Assert.IsFalse(context.IsRegistered<TService>());
        }

        public static void AssertRegisteredNamed<TService>(this IComponentContext context, string service, string message = "Expected named component was not registered.")
        {
            Assert.IsTrue(context.IsRegisteredWithName(service, typeof(TService)), message);
        }

        public static void AssertNotRegisteredNamed<TService>(this IComponentContext context, string service, string message = "Named component was registered unexpectedly.")
        {
            Assert.IsFalse(context.IsRegisteredWithName(service, typeof(TService)), message);
        }
    }
}
