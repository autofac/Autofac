using System;
using Autofac.Core;
using Xunit;

namespace Autofac.Specification.Test.Lifetime
{
    public class InstancePerMatchingLifetimeScope
    {
        [Fact]
        public void ChildOfNamedScopeGetsSharedInstance()
        {
            var contextName = "ctx";

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().InstancePerMatchingLifetimeScope(contextName);
            var container = cb.Build();

            var ctx1 = container.BeginLifetimeScope(contextName);
            var child = ctx1.BeginLifetimeScope();

            Assert.Equal(ctx1.Resolve<object>(), child.Resolve<object>());
        }

        [Fact]
        public void MustHaveMatchingScope()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<object>().InstancePerMatchingLifetimeScope("ctx");
            var container = cb.Build();

            var ctx1 = container.BeginLifetimeScope();
            Assert.Throws<DependencyResolutionException>(() => ctx1.Resolve<object>());
        }

        [Fact]
        public void SharingWithinNamedScope()
        {
            var contextName = "ctx";

            var cb = new ContainerBuilder();
            cb.RegisterType<object>().InstancePerMatchingLifetimeScope(contextName);
            var container = cb.Build();

            var ctx1 = container.BeginLifetimeScope(contextName);
            var ctx2 = container.BeginLifetimeScope(contextName);

            Assert.Equal(ctx1.Resolve<object>(), ctx1.Resolve<object>());
            Assert.NotEqual(ctx1.Resolve<object>(), ctx2.Resolve<object>());
        }
    }
}
