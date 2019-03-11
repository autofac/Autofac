using System;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class SingleInstance
    {
        private interface IA
        {
        }

        public void TypeAsSingleInstance()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>()
                .As<IA>()
                .SingleInstance();
            var c = cb.Build();
            var a1 = c.Resolve<IA>();
            var a2 = c.Resolve<IA>();

            Assert.NotNull(a1);
            Assert.Same(a1, a2);
        }

        private class A : IA
        {
        }
    }
}
