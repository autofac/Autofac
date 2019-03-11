using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void UpdateExcludesDefaultModules()
        {
            var builder = new ContainerBuilder();
            var container = new Container();
#pragma warning disable CS0618
            builder.Update(container);
#pragma warning restore CS0618
            Assert.False(container.IsRegistered<IEnumerable<object>>());
        }
    }
}