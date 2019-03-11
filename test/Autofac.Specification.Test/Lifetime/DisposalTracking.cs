using System;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class DisposalTracking
    {
        private interface IA
        {
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

        [Fact]
        public void TypeAsSingleInstanceDisposedWithContainer()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>()
                .As<IA>()
                .SingleInstance();
            var c = cb.Build();
            var a = c.Resolve<IA>();
            c.Dispose();
            Assert.True(((A)a).IsDisposed);
        }

        private class A : DisposeTracker, IA
        {
        }
    }
}
