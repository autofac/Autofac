// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Builder;
using Autofac.Core;
using Xunit;

namespace Autofac.Specification.Test.Features
{
    public class StartableTests
    {
        private interface IMyService
        {
        }

        private interface IStartableDependency
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

        [Fact]
        public void Startable_WhenChildScopeBegins_NewStartableComponentsAreStarted()
        {
            var startable = new Startable();
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(startable).As<IStartable>());
            Assert.True(startable.StartCount > 0);
        }

        [Fact]
        public void Startable_WhenNoStartIsSpecified_StartableComponentsAreIgnoredInChildLifetimeScope()
        {
            var startable = new Startable();
            var builder = new ContainerBuilder();
            var container = builder.Build(ContainerBuildOptions.IgnoreStartableComponents);
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(startable).As<IStartable>());
            Assert.False(startable.StartCount > 0);
        }

        [Fact]
        public void Startable_WhenNoStartIsSpecified_StartableComponentsAreIgnoredInContainer()
        {
            var startable = new Startable();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(startable).As<IStartable>();
            builder.Build(ContainerBuildOptions.IgnoreStartableComponents);
            Assert.False(startable.StartCount > 0);
        }

        [Fact]
        public void Startable_WhenStartableCreatesChildScope_NoExceptionIsThrown()
        {
            // Issue #916
            var builder = new ContainerBuilder();
            builder.RegisterType<StartableCreatesLifetimeScope>().As<IStartable>().SingleInstance();

            // Assert.DoesNotThrow, basically.
            builder.Build();
        }

        [Fact]
        public void Startable_WhenStartIsSpecified_StartableComponentsAreStarted()
        {
            var startable = new Startable();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(startable).As<IStartable>();
            builder.Build();
            Assert.True(startable.StartCount > 0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Startable_WhenTheContainerIsBuilt_StartableComponentsAreStartedInDependencyOrder(bool ignoreStartableComponents)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<StartableTakesDependency>().AsSelf()
                .SingleInstance().As<IStartable>();

            builder.RegisterType<ComponentTakesStartableDependency>()
                .WithParameter("expectStarted", !ignoreStartableComponents)
                .AsSelf()
                .As<IStartable>();

            var container = builder.Build(ignoreStartableComponents ? ContainerBuildOptions.IgnoreStartableComponents : ContainerBuildOptions.None);
            container.Resolve<ComponentTakesStartableDependency>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Startable_WhenTheContainerIsBuilt_StartableComponentsThatDependOnAutoActivateComponents_AreNotStartedTwice(bool isSingleton)
        {
            var builder = new ContainerBuilder();
            var expectedStartCount = isSingleton ? 1 : 2;
            var dependencyRegistration = builder.RegisterType<StartableDependency>().As<IStartableDependency>().AutoActivate();
            if (isSingleton)
            {
                dependencyRegistration.SingleInstance();
            }

            builder.RegisterType<StartableTakesDependency>().AsSelf().As<IStartable>();

            StartableDependency.Count = 0;
            builder.Build();
            Assert.Equal(expectedStartCount, StartableDependency.Count);
        }

        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0060

        private class ComponentTakesStartableDependency : IStartable
        {
            public ComponentTakesStartableDependency(StartableTakesDependency dependency, bool expectStarted)
            {
                Assert.Equal(expectStarted, dependency.WasStarted);
            }

            public void Start()
            {
            }
        }

        public sealed class MyComponent : IMyService
        {
        }

        public sealed class MyComponent2
        {
        }

        private class Startable : IStartable
        {
            public int StartCount { get; private set; }

            public void Start()
            {
                StartCount++;
            }
        }

        // Issue #916
        private class StartableCreatesLifetimeScope : IStartable
        {
            private readonly ILifetimeScope _scope;

            public StartableCreatesLifetimeScope(ILifetimeScope scope)
            {
                _scope = scope;
            }

            public void Start()
            {
                using (var nested = _scope.BeginLifetimeScope("tag", b => { }))
                {
                }

                using (var nested = _scope.BeginLifetimeScope(b => { }))
                {
                }

                using (var nested = _scope.BeginLifetimeScope("tag"))
                {
                }

                using (var nested = _scope.BeginLifetimeScope())
                {
                }
            }
        }

        private class StartableDependency : IStartableDependency
        {
            public StartableDependency()
            {
                Count++;
            }

            public static int Count { get; set; } = 0;
        }

        private class StartableTakesDependency : IStartable
        {
            public StartableTakesDependency(IStartableDependency[] dependencies)
            {
            }

            public bool WasStarted { get; private set; }

            public void Start()
            {
                WasStarted = true;
            }
        }

#pragma warning disable IDE0060

    }
}
