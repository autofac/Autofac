using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Autofac.Test.Core.Registration
{
    public class ExternalRegistrySourceTests
    {
        public interface IServiceA
        {
        }

        public interface IServiceB
        {
        }

        public class ClassA : IServiceA, IServiceB
        {
        }

        public class ClassB : IServiceA
        {
        }

        [Fact]
        public void OneTypeImplementTwoInterfaces_OtherObjectsImplementingOneOfThoseInterfaces_CanBeResolved()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType(typeof(ClassA)).As(typeof(IServiceA), typeof(IServiceB));
            builder.RegisterType(typeof(ClassB)).As(typeof(IServiceA));

            var container = builder.Build();
            var lifetime = container.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            lifetime.Resolve<IServiceB>();

            var allImplementationsOfServiceA = lifetime.Resolve<IEnumerable<IServiceA>>();
            Assert.Equal(2, allImplementationsOfServiceA.Count());
        }

        // Issue #960
        [Fact]
        public void TwoLayersOfExternalRegistration_OnDisposingInnerLayer_OuterLayerRemains()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ClassA>().InstancePerLifetimeScope();

            // Root has the main registration.
            var root = builder.Build();

            // Middle has ExternalRegistration pointing upwards.
            var middle = root.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            middle.Resolve<ClassA>();

            // Child has ExternalRegistration pointing upwards.
            var child = middle.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            child.Resolve<ClassA>();

            // This should only dispose the registration in child, not the one in middle.
            child.Dispose();

            // We check by trying to use the registration in middle.
            // If too much got disposed, this will throw ObjectDisposedException.
            middle.Resolve<ClassA>();
        }
    }
}
