using System;
using Autofac.Core;
using Xunit;

namespace Autofac.Specification.Test.Features
{
    public class StartableTests
    {
        public interface IMyService
        {
        }

        [Fact]
        public void AutoActivate_CanBeAddedToChildLifetimeScope()
        {
            // Issue #567
            var singletonCount = 0;
            var container = new ContainerBuilder().Build();
            using (var scope = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>().As<IMyService>().SingleInstance().AutoActivate().OnActivated(e => singletonCount++)))
            {
                Assert.Equal(1, singletonCount);
            }
        }

        [Fact]
        public void AutoActivate_InvalidLifetimeConflictsWithAutoStart()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().InstancePerMatchingLifetimeScope("foo").AutoActivate();
            Assert.Throws<DependencyResolutionException>(() => builder.Build());
        }

        [Fact]
        public void AutoActivate_MultipleAutoStartFlagsOnlyStartTheComponentOnce()
        {
            var instanceCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().AutoActivate().AutoActivate().AutoActivate().OnActivated(e => instanceCount++);
            builder.Build();
            Assert.Equal(1, instanceCount);
        }

        [Fact]
        public void AutoActivate_RegistrationsAddedToChildDoNotDoubleActivateParent()
        {
            // Issue #567
            var parentCount = 0;
            var childCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().AutoActivate().OnActivated(e => parentCount++);
            var container = builder.Build();
            using (var scope = container.BeginLifetimeScope(b => b.RegisterType<MyComponent>().AutoActivate().OnActivated(e => childCount++)))
            {
                using (var scope2 = scope.BeginLifetimeScope(b => b.RegisterInstance("x")))
                {
                    Assert.Equal(1, childCount);
                    Assert.Equal(1, parentCount);
                }
            }
        }

        [Fact]
        public void AutoActivate_ResolvesComponentsAutomatically()
        {
            var singletonCount = 0;
            var instanceCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent>().As<IMyService>().SingleInstance().AutoActivate().OnActivated(e => singletonCount++);
            builder.RegisterType<MyComponent2>().AutoActivate().OnActivated(e => instanceCount++);
            builder.Build();
            Assert.Equal(1, singletonCount);
            Assert.Equal(1, instanceCount);
        }

        public sealed class MyComponent : IMyService
        {
        }

        public sealed class MyComponent2
        {
        }
    }
}
