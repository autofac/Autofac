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
            IContainer emptyContainer = new ContainerBuilder().Build();

            var lifetimeScope = emptyContainer.BeginLifetimeScope(b =>
                b.RegisterAssemblyTypes(this.GetType().GetTypeInfo().Assembly)
                    .AssignableTo(typeof(IMyService)));

            var numberOfServicesAdded = lifetimeScope.ComponentRegistry.Registrations.Count() -
                                        emptyContainer.ComponentRegistry.Registrations.Count();

            Assert.Equal(1, numberOfServicesAdded);
            object obj;
            Assert.True(lifetimeScope.TryResolve(typeof(MyComponent), out obj));
            Assert.False(lifetimeScope.TryResolve(typeof(MyComponent2), out obj));
        }

        public sealed class MyComponent : IMyService
        {
        }

        public sealed class MyComponent2
        {
        }
    }
}
