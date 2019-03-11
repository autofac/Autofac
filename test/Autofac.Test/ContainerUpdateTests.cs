using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Xunit;

namespace Autofac.Test
{
    // Container update is marked obsolete. This fixture will end up getting
    // removed when update is removed.
    public class ContainerUpdateTests
    {
        private interface IMyService
        {
        }

        [Fact]
        public void AutoActivate_ContainerUpdateAutoActivatesNewComponents()
        {
            // Issue #454: ContainerBuilder.Update() doesn't activate AutoActivate components.
            var instanceCount = 0;
            var container = new ContainerBuilder().Build();
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().AutoActivate().OnActivated(e => instanceCount++);
#pragma warning disable CS0618
            builder.Update(container);
#pragma warning restore CS0618
            Assert.Equal(1, instanceCount);
        }

        [Fact]
        public void AutoActivate_ContainerUpdateDoesNotAutoActivateExistingComponents()
        {
            // Issue #454: ContainerBuilder.Update() shouldn't re-activate existing AutoActivate components.
            var firstCount = 0;
            var builder = new ContainerBuilder();
            builder.RegisterType<MyComponent2>().AutoActivate().OnActivated(e => firstCount++);
            var container = builder.Build();
            Assert.Equal(1, firstCount);

            var secondCount = 0;
            var builder2 = new ContainerBuilder();
            builder2.RegisterType<MyComponent>().AutoActivate().OnActivated(e => secondCount++);
#pragma warning disable CS0618
            builder2.Update(container);
#pragma warning restore CS0618
            Assert.Equal(1, firstCount);
            Assert.Equal(1, secondCount);
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

        private sealed class MyComponent : IMyService
        {
        }

        private sealed class MyComponent2
        {
        }
    }
}
