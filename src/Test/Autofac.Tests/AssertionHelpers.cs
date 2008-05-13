using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Autofac.Tests
{
    static class AssertionHelpers
    {
        public static void AssertRegistered<TService>(this IContext context)
        {
            Assert.IsTrue(context.IsRegistered<TService>());
        }

        public static void AssertNotRegistered<TService>(this IContext context)
        {
            Assert.IsFalse(context.IsRegistered<TService>());
        }

        public static void AssertRegistered(this IContext context, string service)
        {
            Assert.IsTrue(context.IsRegistered(service));
        }

        public static void AssertNotRegistered(this IContext context, string service)
        {
            Assert.IsFalse(context.IsRegistered(service));
        }
    }
}
