// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class InstancePerDependencyTests
    {
        private interface IA
        {
        }

        [Fact]
        public void TypeAsInstancePerDependency()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>().As<IA>();
            var c = cb.Build();
            var ctx = c.BeginLifetimeScope();
            var a1 = ctx.Resolve<IA>();
            var a2 = ctx.Resolve<IA>();
            var a3 = c.Resolve<IA>();

            Assert.NotNull(a1);
            Assert.NotNull(a2);
            Assert.NotNull(a3);
            Assert.NotSame(a1, a2);
            Assert.NotSame(a1, a3);
        }

        [Fact]
        public void TypeAsInstancePerDependencyDisposedWithScope()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>().As<IA>();
            var c = cb.Build();
            var ctx = c.BeginLifetimeScope();
            var a1 = ctx.Resolve<IA>();
            var a2 = ctx.Resolve<IA>();
            ctx.Dispose();

            Assert.NotNull(a1);
            Assert.NotSame(a1, a2);
            Assert.True(((A)a1).IsDisposed);
            Assert.True(((A)a2).IsDisposed);
        }

        private class A : DisposeTracker, IA
        {
        }
    }
}
