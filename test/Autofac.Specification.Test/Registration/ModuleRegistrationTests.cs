using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

                this.ConfigureCalled = true;
                builder.RegisterType<object>().SingleInstance();
            }
        }
    }
}
