using System;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    /// <summary>
    /// Tests for fairly simple/straightforward type-as-self or
    /// type-as-service registrations.
    /// </summary>
    public class TypeRegistrationTests
    {
        private interface IA
        {
        }

        private interface IB
        {
        }

        private interface IC
        {
        }

        [Fact]
        public void RegisterTypeAsSupportedAndUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>().As<IA, IB>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void RegisterTypeAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<string>().As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }

        [Fact]
        public void TypeRegisteredOnlyWithServiceNotRegisteredAsSelf()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>().As<IA>();
            var c = cb.Build();
            Assert.False(c.IsRegistered<A>());
        }

        [Fact]
        public void TypeRegisteredWithMultipleServicesCanBeResolved()
        {
            var target = new ContainerBuilder();
            target.RegisterType<ABC>()
                .As<IA, IB, IC>()
                .SingleInstance();
            var container = target.Build();
            var a = container.Resolve<IA>();
            var b = container.Resolve<IB>();
            var c = container.Resolve<IC>();
            Assert.NotNull(a);
            Assert.Same(a, b);
            Assert.Same(b, c);
        }

        [Fact]
        public void TypeRegisteredWithoutServiceCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>();
            var c = cb.Build();
            var a = c.Resolve<A>();
            Assert.NotNull(a);
            Assert.IsType<A>(a);
        }

        [Fact]
        public void TypeRegisteredWithSingleServiceCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A>().As<IA>();
            var c = cb.Build();
            var a = c.Resolve<IA>();
            Assert.NotNull(a);
            Assert.IsType<A>(a);
        }

        private class A : IA
        {
        }

        private class ABC : IA, IB, IC
        {
        }
    }
}
