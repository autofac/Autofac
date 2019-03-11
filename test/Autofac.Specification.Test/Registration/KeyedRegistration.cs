using System;
using Autofac.Features.Indexed;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class KeyedRegistration
    {
        [Fact]
        public void TypeRegisteredWithKey()
        {
            var key = new object();

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Keyed<object>(key);

            var c = cb.Build();

            object o1;
            Assert.True(c.TryResolveKeyed(key, typeof(object), out o1));
            Assert.NotNull(o1);

            object o2;
            Assert.False(c.TryResolve(typeof(object), out o2));
        }

        [Fact]
        public void TypeRegisteredWithName()
        {
            var name = "object.registration";

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().Named<object>(name);

            var c = cb.Build();

            object o1;
            Assert.True(c.TryResolveNamed(name, typeof(object), out o1));
            Assert.NotNull(o1);

            object o2;
            Assert.False(c.TryResolve(typeof(object), out o2));
        }
    }
}
