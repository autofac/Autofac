using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class AssemblyScanningTests
    {
        public interface IMyService
        {
        }

        [Fact]
        public void OnlyServicesAssignableToASpecificTypeAreRegisteredFromAssemblies()
        {
            var container =
                new ContainerBuilder()
                    .Build()
                    .BeginLifetimeScope(b => b.RegisterAssemblyTypes(this.GetType().GetTypeInfo().Assembly).AssignableTo(typeof(IMyService)));

            Assert.Single(container.ComponentRegistry.Registrations);
            object obj;
            Assert.True(container.TryResolve(typeof(MyComponent), out obj));
            Assert.False(container.TryResolve(typeof(MyComponent2), out obj));
        }

        public sealed class MyComponent : IMyService
        {
        }

        public sealed class MyComponent2
        {
        }
    }
}
