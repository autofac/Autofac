using System;
using Autofac.Specification.Test.Util;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class SingleInstanceTests
    {
        private interface IA
        {
        }

        [Fact]
        public void TypeAsSingleInstance()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>()
                .As<IA>()
                .SingleInstance();
            var c = cb.Build();
            var a1 = c.Resolve<IA>();
            var a2 = c.Resolve<IA>();
            var a3 = c.BeginLifetimeScope().Resolve<IA>();
            var a4 = c.BeginLifetimeScope().Resolve<IA>();

            Assert.NotNull(a1);
            Assert.Same(a1, a2);
            Assert.Same(a1, a3);
            Assert.Same(a1, a4);
        }

        [Fact]
        public void TypeAsSingleInstanceDisposedWithContainer()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>()
                .As<IA>()
                .SingleInstance();
            var c = cb.Build();
            var a1 = c.Resolve<IA>();
            var lifetime = c.BeginLifetimeScope();
            var a2 = lifetime.Resolve<IA>();
            lifetime.Dispose();
            Assert.False(((A)a1).IsDisposed);
            c.Dispose();
            Assert.True(((A)a1).IsDisposed);
        }

        private class A : DisposeTracker, IA
        {
        }
    }
}
