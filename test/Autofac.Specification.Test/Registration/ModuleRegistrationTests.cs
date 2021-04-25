// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core.Registration;
using Autofac.Test.Scenarios.ScannedAssembly;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class ModuleRegistrationTests
    {
        [Fact]
        public void ModuleCanRegisterModule()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<Module2>();
            var container = builder.Build();
            Assert.NotNull(container.Resolve<object>());
        }

        [Fact]
        public void RegisterAssemblyModules()
        {
            var assembly = typeof(AComponent).GetTypeInfo().Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(assembly);
            var container = builder.Build();

            Assert.True(container.IsRegistered<AComponent>());
            Assert.True(container.IsRegistered<BComponent>());
        }

        [Fact]
        public void RegisterAssemblyModulesChainedToRegisterModule()
        {
            var assembly = typeof(AComponent).GetTypeInfo().Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(assembly).RegisterModule<ObjectModule>();
            var container = builder.Build();

            Assert.True(container.IsRegistered<AComponent>());
            Assert.True(container.IsRegistered<BComponent>());
            Assert.True(container.IsRegistered<object>());
        }

        [Fact]
        public void RegisterAssemblyModulesOfBaseGenericType()
        {
            var assembly = typeof(AComponent).GetTypeInfo().Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules<ModuleBase>(assembly);
            var container = builder.Build();

            Assert.True(container.IsRegistered<AComponent>());
            Assert.True(container.IsRegistered<BComponent>());
        }

        [Fact]
        public void RegisterAssemblyModulesOfBaseType()
        {
            var assembly = typeof(AComponent).GetTypeInfo().Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(ModuleBase), assembly);
            var container = builder.Build();

            Assert.True(container.IsRegistered<AComponent>());
            Assert.True(container.IsRegistered<BComponent>());
        }

        [Fact]
        public void RegisterAssemblyModulesOfGenericType()
        {
            var assembly = typeof(AComponent).GetTypeInfo().Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules<AModule>(assembly);
            var container = builder.Build();

            Assert.True(container.IsRegistered<AComponent>());
            Assert.False(container.IsRegistered<BComponent>());
        }

        [Fact]
        public void RegisterAssemblyModulesOfType()
        {
            var assembly = typeof(AComponent).GetTypeInfo().Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(AModule), assembly);
            var container = builder.Build();

            Assert.True(container.IsRegistered<AComponent>());
            Assert.False(container.IsRegistered<BComponent>());
        }

        [Fact]
        public void RegisterModule()
        {
            var mod = new ObjectModule();
            var target = new ContainerBuilder();
            target.RegisterModule(mod);
            Assert.False(mod.ConfigureCalled);
            var container = target.Build();
            Assert.True(mod.ConfigureCalled);
            Assert.True(container.IsRegistered<object>());
        }

        [Fact]
        public void ModuleCanContainDecorators()
        {
            var mod = new DecoratingModule();
            var target = new ContainerBuilder();
            target.RegisterType<S1>().As<I1>();
            target.RegisterModule(mod);
            var container = target.Build();
            Assert.IsType<Decorator1>(container.Resolve<I1>());
        }

        [Fact]
        public void ModuleCanContainComposites()
        {
            var mod = new CompositingModule();
            var target = new ContainerBuilder();
            target.RegisterType<S1>().As<I1>();
            target.RegisterModule(mod);
            var container = target.Build();
            Assert.IsType<Composite1>(container.Resolve<I1>());
        }

        [Fact]
        public void OnlyIf_RequiresPredicate()
        {
            var mod = new ObjectModule();
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterModule(mod).OnlyIf(null));
        }

        [Fact]
        public void OnlyIf_RequiresRegistrar()
        {
            var mod = new ObjectModule();
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => ModuleRegistrationExtensions.OnlyIf(null, reg => true));
        }

        [Fact]
        public void OnlyIf_PreventsModuleRegistration()
        {
            var mod = new ObjectModule();
            var builder = new ContainerBuilder();
            builder.RegisterModule(mod).OnlyIf(reg => false);
            var container = builder.Build();
            Assert.False(mod.ConfigureCalled);
        }

        [Fact]
        public void OnlyIf_AllowsModuleRegistration()
        {
            var mod = new ObjectModule();
            var builder = new ContainerBuilder();
            builder.RegisterModule(mod).OnlyIf(reg => true);
            var container = builder.Build();
            Assert.True(mod.ConfigureCalled);
        }

        [Fact]
        public void OnlyIf_Stacks_In_ReverseOrder_Allow()
        {
            var counter = 0;
            var builder = new ContainerBuilder();
            var objModule1 = new ObjectModule();
            var objModule2 = new ObjectModule();
            builder.RegisterModule(objModule1)
                   .RegisterModule(objModule2)
                   .OnlyIf(regBuilder => counter++ == 1)
                   .OnlyIf(regBuilder => counter++ == 0);
            var container = builder.Build();
            Assert.True(objModule1.ConfigureCalled);
            Assert.True(objModule2.ConfigureCalled);
            Assert.Equal(2, counter);
        }

        [Fact]
        public void OnlyIf_Stacks_In_ReverseOrder_Deny()
        {
            var counter = 0;
            var builder = new ContainerBuilder();
            var objModule1 = new ObjectModule();
            var objModule2 = new ObjectModule();
            builder.RegisterModule(objModule1)

                   // Shouldn't run because last OnlyIf fails
                   .OnlyIf(regBuilder => counter++ != 1)
                   .RegisterModule(objModule2)
                   .OnlyIf(regBuilder => counter++ != 0);
            var container = builder.Build();
            Assert.False(objModule1.ConfigureCalled);
            Assert.False(objModule2.ConfigureCalled);
            Assert.Equal(1, counter);
        }

        [Fact]
        public void IfNotRegistered_IgnoresOtherRegistrationsInSameChain()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ObjectModule())
                   .RegisterModule(new StringModule())
                   .IfNotRegistered(typeof(object));
            var container = builder.Build();
            Assert.NotNull(container.Resolve<object>());
            Assert.Equal("foo", container.Resolve<string>());
        }

        [Fact]
        public void IfNotRegistered_RequiresRegistrar()
        {
            var mod = new ObjectModule();
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => ModuleRegistrationExtensions.IfNotRegistered(null, typeof(object)));
        }

        [Fact]
        public void IfNotRegistered_RequiresType()
        {
            var mod = new ObjectModule();
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterModule(mod).IfNotRegistered(null));
        }

        [Fact]
        public void IfNotRegistered_PreventsAllRegistrationsInSameChain()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("bar");
            builder.RegisterModule(new ObjectModule())
                   .RegisterModule(new StringModule())
                   .IfNotRegistered(typeof(string));
            var container = builder.Build();
            Assert.Throws<ComponentNotRegisteredException>(() => container.Resolve<object>());
            Assert.Equal("bar", container.Resolve<string>());
        }

        // Disable "unused parameter" warnings for test types.
#pragma warning disable IDE0060

        private class Module1 : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<object>();
            }
        }

        private class Module2 : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterModule<Module1>();
            }
        }

        private class DecoratingModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterDecorator<Decorator1, I1>();
            }
        }

        private class CompositingModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterComposite<Composite1, I1>();
            }
        }

        private interface I1
        {
        }

        private class S1 : I1
        {
        }

        private class Decorator1 : I1
        {
            public Decorator1(I1 instance)
            {
            }
        }

        private class Composite1 : I1
        {
            public Composite1(IEnumerable<I1> instance)
            {
            }
        }

        internal class ObjectModule : Module
        {
            public bool ConfigureCalled { get; private set; }

            protected override void Load(ContainerBuilder builder)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException(nameof(builder));
                }

                ConfigureCalled = true;
                builder.RegisterType<object>().SingleInstance();
            }
        }

        internal class StringModule : Module
        {
            public bool ConfigureCalled { get; private set; }

            protected override void Load(ContainerBuilder builder)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException(nameof(builder));
                }

                ConfigureCalled = true;
                builder.RegisterInstance("foo");
            }
        }

#pragma warning disable IDE0060

    }
}
