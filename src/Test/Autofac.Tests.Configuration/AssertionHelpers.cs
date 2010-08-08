using NUnit.Framework;
using Autofac.Core;

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

        public static void AssertRegistered<TService>(this IComponentContext context, string service)
        {
            Assert.IsTrue(context.IsRegisteredNamed(service, typeof(TService)));
        }

        public static void AssertNotRegistered<TService>(this IComponentContext context, string service)
        {
            Assert.IsFalse(context.IsRegisteredNamed(service, typeof(TService)));
        }
    }
}
