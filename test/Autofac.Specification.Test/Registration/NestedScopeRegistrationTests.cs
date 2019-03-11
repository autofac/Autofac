using System;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class NestedScopeRegistrationTests
    {
        private interface IMyService
        {
        }

        [Fact]
        public void RegistrationsMadeInConfigureExpressionAreAddedToContainer()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var ls = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>().As<IMyService>());

            var component = ls.Resolve<IMyService>();
            Assert.True(component is MyComponent);
        }

        private sealed class MyComponent : IMyService
        {
        }
    }
}
