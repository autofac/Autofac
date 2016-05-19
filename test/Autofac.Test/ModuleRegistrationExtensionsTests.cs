using System;
using System.Reflection;
using Autofac.Test.Scenarios.ScannedAssembly;
using Xunit;

namespace Autofac.Test
{
    public class ModuleRegistrationExtensionsTests
    {
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

#if !NET451
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
        public void RegisterAssemblyModulesOfBaseGenericType()
        {
            var assembly = typeof(AComponent).GetTypeInfo().Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules<ModuleBase>(assembly);
            var container = builder.Build();

            Assert.True(container.IsRegistered<AComponent>());
            Assert.True(container.IsRegistered<BComponent>());
        }
#endif

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
        public void RegisterAssemblyModulesOfBaseType()
        {
            var assembly = typeof(AComponent).GetTypeInfo().Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(ModuleBase), assembly);
            var container = builder.Build();

            Assert.True(container.IsRegistered<AComponent>());
            Assert.True(container.IsRegistered<BComponent>());
        }

        internal class ObjectModule : Module
        {
            public bool ConfigureCalled { get; private set; }

            protected override void Load(ContainerBuilder builder)
            {
                if (builder == null) throw new ArgumentNullException("builder");
                ConfigureCalled = true;
                builder.RegisterType<object>().SingleInstance();
            }
        }
    }
}
