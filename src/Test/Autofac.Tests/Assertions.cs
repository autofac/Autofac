using NUnit.Framework;
using Autofac.Core;
using System;

namespace Autofac.Tests
{
    static class Assertions
    {
        public static void AssertRegistered<TService>(this IComponentContext context)
        {
            Assert.IsTrue(context.IsRegistered<TService>());
        }

        public static void AssertNotRegistered<TService>(this IComponentContext context)
        {
            Assert.IsFalse(context.IsRegistered<TService>());
        }

        public static void AssertRegistered(this IComponentContext context, string service)
        {
            Assert.IsTrue(context.IsRegistered(service));
        }

        public static void AssertNotRegistered(this IComponentContext context, string service)
        {
            Assert.IsFalse(context.IsRegistered(service));
        }

        public static void AssertSharing<TComponent>(this IComponentContext context, InstanceSharing sharing)
        {
            var cr = context.RegistrationFor<TComponent>();
            Assert.AreEqual(sharing, cr.Sharing);
        }

        public static void AssertLifetime<TComponent, TLifetime>(this IComponentContext context)
        {
            var cr = context.RegistrationFor<TComponent>();
            Assert.IsInstanceOf<TLifetime>(cr.Lifetime);
        }

        public static void AssertOwnership<TComponent>(this IComponentContext context, InstanceOwnership ownership)
        {
            var cr = context.RegistrationFor<TComponent>();
            Assert.AreEqual(ownership, cr.Ownership);
        }

        public static IComponentRegistration RegistrationFor<TComponent>(this IComponentContext context)
        {
            IComponentRegistration r;
            Assert.IsTrue(context.ComponentRegistry.TryGetRegistration(new TypedService(typeof(TComponent)), out r));
            return r;
        }

        public static void AssertThrows<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(TException))
                    Assert.Fail("Expected {0}, but caught {1}.", typeof(TException), ex);
                
                return;
            }

            Assert.Fail("Expected {0}, but no exception thrown.", typeof(TException));
        }
    }
}
