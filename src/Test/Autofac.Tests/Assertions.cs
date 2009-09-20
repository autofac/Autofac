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
