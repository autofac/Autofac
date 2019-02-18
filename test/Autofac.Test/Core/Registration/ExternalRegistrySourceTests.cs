using System;
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

        public class ClassA : IServiceA, IServiceB, IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public class ClassB : IServiceA, IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
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
            var rootInstance = root.Resolve<ClassA>();

            // Middle has ExternalRegistration pointing upwards.
            var middle = root.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            var middleInstance = middle.Resolve<ClassA>();

            // Child has ExternalRegistration pointing upwards.
            var child = middle.BeginLifetimeScope(cb => cb.Register(_ => new object()));
            var childInstance = child.Resolve<ClassA>();

            Assert.NotSame(rootInstance, middleInstance);
            Assert.NotSame(middleInstance, childInstance);
            Assert.NotSame(rootInstance, childInstance);

            // This should only dispose the registration in child, not the one in middle or root.
            child.Dispose();
            Assert.True(childInstance.IsDisposed);
            Assert.False(middleInstance.IsDisposed);
            Assert.False(rootInstance.IsDisposed);

            // We check by trying to use the registration in middle.
            // If too much got disposed, this will throw ObjectDisposedException.
            Assert.Same(middleInstance, middle.Resolve<ClassA>());
            Assert.Same(rootInstance, root.Resolve<ClassA>());

            // Middle and child should now be disposed.
            middle.Dispose();
            Assert.True(childInstance.IsDisposed);
            Assert.True(middleInstance.IsDisposed);
            Assert.False(rootInstance.IsDisposed);

            // Now all should be disposed.
            root.Dispose();
            Assert.True(childInstance.IsDisposed);
            Assert.True(middleInstance.IsDisposed);
            Assert.True(rootInstance.IsDisposed);
        }
    }
}
