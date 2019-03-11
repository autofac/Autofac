using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;
using Xunit;

namespace Autofac.Specification.Test
{
    public class ContainerBuilderTests
    {
        [Fact]
        public void BuildCallbacksInvokedWhenContainerBuilt()
        {
            var called = 0;

            void BuildCallback(IContainer c)
            {
                called++;
            }

            new ContainerBuilder()
                .RegisterBuildCallback(BuildCallback)
                .RegisterBuildCallback(BuildCallback)
                .Build();

            Assert.Equal(2, called);
        }

        [Fact]
        public void BuildCallbacksInvokedWhenRegisteredInModuleLoad()
        {
            var module = new BuildCallbackModule();

            var builder = new ContainerBuilder();
            builder.RegisterModule(module);
            builder.Build();

            Assert.Equal(2, module.Called);
        }

        [Fact]
        public void CtorCreatesDefaultPropertyBag()
        {
            var builder = new ContainerBuilder();
            Assert.NotNull(builder.Properties);
        }

        [Fact]
        public void DefaultModulesCanBeExcluded()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);
            Assert.False(container.IsRegistered<IEnumerable<object>>());
        }

        [Fact]
        public void OnlyAllowBuildOnce()
        {
            var target = new ContainerBuilder();
            target.Build();
            Assert.Throws<InvalidOperationException>(() => target.Build());
        }

        public class BuildCallbackModule : Module
        {
            public int Called { get; private set; }

            protected override void Load(ContainerBuilder builder)
            {
                void BuildCallback(IContainer c)
                {
                    this.Called++;
                }

                builder.RegisterBuildCallback(BuildCallback)
                    .RegisterBuildCallback(BuildCallback);
            }
        }
    }
}
