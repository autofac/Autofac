// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

            void BuildCallback(ILifetimeScope c)
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
        public void BuildCallbacksInvokedWhenRegisteredInBeginLifetimeScope()
        {
            var called = 0;

            void BuildCallback(ILifetimeScope c)
            {
                Assert.False(c is IContainer);
                called++;
            }

            var builder = new ContainerBuilder();
            var container = builder.Build();

            var scope = container.BeginLifetimeScope(cfg =>
            {
                cfg.RegisterBuildCallback(BuildCallback);
                cfg.RegisterBuildCallback(BuildCallback);
            });

            Assert.Equal(2, called);
        }

        [Fact]
        public void DifferentBuildCallbacksInvokedWhenRegisteredInBothContainerAndScopes()
        {
            var callOrder = new List<string>();

            void ContainerBuildCallback(ILifetimeScope c)
            {
                Assert.True(c is IContainer);
                callOrder.Add("container");
            }

            void ScopeBuildCallback(ILifetimeScope c, string name)
            {
                Assert.False(c is IContainer);
                callOrder.Add(name);
            }

            var builder = new ContainerBuilder();
            builder.RegisterBuildCallback(ContainerBuildCallback);
            builder.RegisterBuildCallback(ContainerBuildCallback);

            var container = builder.Build();

            var scope = container.BeginLifetimeScope(cfg =>
            {
                cfg.RegisterBuildCallback(sc => ScopeBuildCallback(sc, "scope1"));
                cfg.RegisterBuildCallback(sc => ScopeBuildCallback(sc, "scope1"));
            });

            // Go another level down
            scope.BeginLifetimeScope(cfg =>
            {
                cfg.RegisterBuildCallback(sc => ScopeBuildCallback(sc, "scope2"));
                cfg.RegisterBuildCallback(sc => ScopeBuildCallback(sc, "scope2"));
            });

            Assert.Equal(
                new[]
                {
                    "container",
                    "container",
                    "scope1",
                    "scope1",
                    "scope2",
                    "scope2"
                }, callOrder);
        }

        [Fact]
        public void BuildCallbacksInvokedWhenRegisteredInModuleLoadFromScope()
        {
            var module = new BuildCallbackModule();

            var builder = new ContainerBuilder();
            builder.Build().BeginLifetimeScope(cfg => cfg.RegisterModule(module));

            Assert.Equal(2, module.Called);
        }

        [Fact]
        public void BuildCallbacksInvokedWhenModuleAndNormalRegistered()
        {
            var module = new BuildCallbackModule();
            var called = 0;

            var builder = new ContainerBuilder();
            builder.RegisterBuildCallback(scope => called++);
            builder.RegisterModule(module);

            builder.Build();

            Assert.Equal(2, module.Called);
            Assert.Equal(1, called);
        }

        [Fact]
        public void BuildCallbacksInvokedInScopeWhenModuleAndNormalRegistered()
        {
            var module = new BuildCallbackModule();
            var called = 0;

            var builder = new ContainerBuilder();
            builder.Build().BeginLifetimeScope(cfg =>
            {
                cfg.RegisterBuildCallback(scope => called++);
                cfg.RegisterModule(module);
            });

            Assert.Equal(2, module.Called);
            Assert.Equal(1, called);
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
                void BuildCallback(ILifetimeScope c)
                {
                    Called++;
                }

                builder.RegisterBuildCallback(BuildCallback)
                       .RegisterBuildCallback(BuildCallback);
            }
        }

        private class NestingModule : Module
        {
            public bool OuterBuildCallback { get; set; }

            public bool InnerBuildCallback { get; set; }

            protected override void Load(ContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterBuildCallback(container =>
                {
                    OuterBuildCallback = true;
                    var appScope = container.BeginLifetimeScope(nestedBuilder =>
                    {
                        nestedBuilder.RegisterBuildCallback(c => InnerBuildCallback = true);
                    });
                });
            }
        }

        [Fact]
        public void CheckLifetimeScopeBuilderNestedInsideModule()
        {
            var builder = new ContainerBuilder();
            var mod = new NestingModule();
            builder.RegisterModule(mod);
            builder.Build();
            Assert.True(mod.OuterBuildCallback);
            Assert.True(mod.InnerBuildCallback);
        }
    }
}
