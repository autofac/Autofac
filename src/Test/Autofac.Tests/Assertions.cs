using System.Collections.Generic;
using System.Linq;
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

        public static void AssertRegistered<TService>(this IComponentContext context, string service)
        {
            Assert.IsTrue(context.IsRegisteredWithName<TService>(service));
        }

        public static void AssertNotRegistered<TService>(this IComponentContext context, string service)
        {
            Assert.IsFalse(context.IsRegisteredWithName<TService>(service));
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

        /// <summary>
        /// Looks at all registrations for <typeparamref name="TService"/> and validates that <typeparamref name="TFirstComponent"/> is not overridden by <typeparamref name="TLastComponent"/>.
        /// </summary>
        public static void AssertComponentRegistrationOrder<TService, TFirstComponent, TLastComponent>(this IComponentContext context)
        {
            var forService = new TypedService(typeof(TService));
            var firstComponent = typeof(TFirstComponent);
            var lastComponent = typeof(TLastComponent);

            var registrations = context.ComponentRegistry.RegistrationsFor(forService);

            var types = registrations.LookForComponents(new[] { firstComponent, lastComponent }).ToArray();
            if (types.Length < 2)
            {
                Assert.Fail("Components are not registered");
            }

            var foundFirst = types[0] == firstComponent;
            var foundLast = types[1] == lastComponent;

            if (!(foundFirst && foundLast))
                Assert.Fail("Component '" + lastComponent.Name + "' is overriding component '" + firstComponent.Name + "'");
        }

        private static IEnumerable<Type> LookForComponents(this IEnumerable<IComponentRegistration> registrations, IEnumerable<Type> types)
        {
            return registrations
                .Select(registration => types.FirstOrDefault(type => registration.Activator.LimitType == type))
                .Where(foundType => foundType != null);
        }

    }
}
