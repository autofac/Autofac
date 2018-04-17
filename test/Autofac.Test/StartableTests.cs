using System;
using System.Linq;
using Autofac.Builder;
using Xunit;

namespace Autofac.Test
{
    public class StartableTests
    {
        private interface IStartableDependency
        {
        }

        [Fact]
        public void WhenChildScopeBegins_NewStartableComponentsAreStarted()
        {
            var startable = Mocks.GetStartable();
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(startable).As<IStartable>());
            Assert.True(startable.StartCount > 0);
        }

        [Fact]
        public void WhenNoStartIsSpecified_StartableComponentsAreIgnoredInChildLifetimeScope()
        {
            var startable = Mocks.GetStartable();
            var builder = new ContainerBuilder();
            var container = builder.Build(ContainerBuildOptions.IgnoreStartableComponents);
            var scope = container.BeginLifetimeScope(b => b.RegisterInstance(startable).As<IStartable>());
            Assert.False(startable.StartCount > 0);
        }

        [Fact]
        public void WhenNoStartIsSpecified_StartableComponentsAreIgnoredInContainer()
        {
            var startable = Mocks.GetStartable();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(startable).As<IStartable>();
            builder.Build(ContainerBuildOptions.IgnoreStartableComponents);
            Assert.False(startable.StartCount > 0);
        }

        [Fact]
        public void WhenStartIsSpecified_StartableComponentsAreStarted()
        {
            var startable = Mocks.GetStartable();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(startable).As<IStartable>();
            builder.Build();
            Assert.True(startable.StartCount > 0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WhenTheContainerIsBuilt_StartableComponentsAreStartedInDependencyOrder(bool ignoreStartableComponents)
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
        public void WhenTheContainerIsBuilt_StartableComponentsThatDependOnAutoActivateComponents_AreNotStartedTwice(bool isSingleton)
        {
            var builder = new ContainerBuilder();
            var expectedStartCount = isSingleton ? 1 : 2;
            var dependencyRegistration = builder.RegisterType<StartableDependency>().As<IStartableDependency>().AutoActivate();
            if (isSingleton)
                dependencyRegistration.SingleInstance();

            builder.RegisterType<StartableTakesDependency>().AsSelf().As<IStartable>();

            StartableDependency.Count = 0;
            builder.Build();
            Assert.Equal(expectedStartCount, StartableDependency.Count);
        }

        [Fact]
        public void WhenTheContainerIsUpdated_ExistingStartableComponentsAreNotRestarted()
        {
            var startable1 = Mocks.GetStartable();
            var startable2 = Mocks.GetStartable();

            var builder1 = new ContainerBuilder();
            builder1.RegisterInstance(startable1).As<IStartable>();
            var container = builder1.Build();

            Assert.Equal(1, startable1.StartCount);

            var builder2 = new ContainerBuilder();
            builder2.RegisterInstance(startable2).As<IStartable>();
#pragma warning disable CS0618
            builder2.Update(container);
#pragma warning restore CS0618

            Assert.Equal(1, startable1.StartCount);
            Assert.Equal(1, startable2.StartCount);
        }

        [Fact]
        public void WhenTheContainerIsUpdated_NewStartableComponentsAreStarted()
        {
            // Issue #454: ContainerBuilder.Update() doesn't activate startable components.
            var container = new ContainerBuilder().Build();

            var startable = Mocks.GetStartable();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(startable).As<IStartable>();
#pragma warning disable CS0618
            builder.Update(container);
#pragma warning restore CS0618

            Assert.Equal(1, startable.StartCount);
        }

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

        private class StartableDependency : IStartableDependency
        {
            private static int _count = 0;

            public StartableDependency()
            {
                _count++;
            }

            public static int Count
            {
                get
                {
                    return _count;
                }

                set
                {
                    _count = value;
                }
            }
        }

        private class StartableTakesDependency : IStartable
        {
            public StartableTakesDependency(IStartableDependency[] dependencies)
            {
            }

            public bool WasStarted { get; private set; }

            public void Start()
            {
                this.WasStarted = true;
            }
        }
    }
}
