using System;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class InstancePerDependency
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

            Assert.NotNull(a1);
            Assert.NotNull(a2);
            Assert.NotSame(a1, a2);
        }

        private class A : IA
        {
        }
    }
}
