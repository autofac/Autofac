using System;
using Autofac.Core;
using Xunit;

namespace Autofac.Test
{
    public class ContainerBuilderTests
    {
        [Fact]
        public void RegisterBuildCallbackReturnsBuilderInstance()
        {
            var builder = new ContainerBuilder();
            Assert.Same(builder, builder.RegisterBuildCallback(c => { }));
        }

        [Fact]
        public void RegisterBuildCallbackThrowsWhenProvidedNullCallback()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterBuildCallback(null));
            Assert.Equal("buildCallback", exception.ParamName);
        }

        [Fact]
        public void RegistrationsCanUsePropertyBag()
        {
            var builder = new ContainerBuilder();
            builder.Properties["count"] = 0;
            builder.Register(ctx =>
            {
                // TOTALLY not thread-safe, but illustrates the point.
                var count = (int)ctx.ComponentRegistry.Properties["count"];
                count++;
                ctx.ComponentRegistry.Properties["count"] = count;
                return "incremented";
            }).As<string>();
            var container = builder.Build();

            container.Resolve<string>();
            container.Resolve<string>();

            Assert.Equal(2, container.ComponentRegistry.Properties["count"]);
        }

        [Theory]
        [InlineData(InstanceOwnership.OwnedByLifetimeScope)]
        [InlineData(InstanceOwnership.ExternallyOwned)]
        public void ContainerBuilderPassesRegistrationsOwnership(InstanceOwnership ownership)
        {
            var builder = new ContainerBuilder(ownership);
            builder.RegisterType<object>();
            var container = builder.Build();
            var regExists = container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(object)), out var reg);
            Assert.True(regExists);
            Assert.Equal(ownership, reg.Ownership);
        }
    }
}