using System;
using Autofac.Tests.Scenarios.ScannedAssembly;
using NUnit.Framework;

namespace Autofac.Tests
{
    [TestFixture]
    public class ModuleRegistrationExtensionsTests
    {
        [Test]
        public void RegisterModule()
        {
            var mod = new ObjectModule();
            var target = new ContainerBuilder();
            target.RegisterModule(mod);
            Assert.IsFalse(mod.ConfigureCalled);
            var container = target.Build();
            Assert.IsTrue(mod.ConfigureCalled);
            Assert.IsTrue(container.IsRegistered<object>());
        }

        [Test]
        public void RegisterAssemblyModules()
        {
            var assembly = typeof(AComponent).Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(assembly);
            var container = builder.Build();

            Assert.That(container.IsRegistered<AComponent>(), Is.True);
            Assert.That(container.IsRegistered<BComponent>(), Is.True);
        }

        [Test]
        public void RegisterAssemblyModulesChainedToRegisterModule()
        {
            var assembly = typeof(AComponent).Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(assembly).RegisterModule<ObjectModule>();
            var container = builder.Build();

            Assert.IsTrue(container.IsRegistered<AComponent>());
            Assert.IsTrue(container.IsRegistered<BComponent>());
            Assert.IsTrue(container.IsRegistered<object>());
        }

        [Test]
        public void RegisterAssemblyModulesOfGenericType()
        {
            var assembly = typeof(AComponent).Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules<AModule>(assembly);
            var container = builder.Build();

            Assert.That(container.IsRegistered<AComponent>(), Is.True);
            Assert.That(container.IsRegistered<BComponent>(), Is.False);
        }

        [Test]
        public void RegisterAssemblyModulesOfBaseGenericType()
        {
            var assembly = typeof(AComponent).Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules<ModuleBase>(assembly);
            var container = builder.Build();

            Assert.That(container.IsRegistered<AComponent>(), Is.True);
            Assert.That(container.IsRegistered<BComponent>(), Is.True);
        }

        [Test]
        public void RegisterAssemblyModulesOfType()
        {
            var assembly = typeof(AComponent).Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(AModule), assembly);
            var container = builder.Build();

            Assert.That(container.IsRegistered<AComponent>(), Is.True);
            Assert.That(container.IsRegistered<BComponent>(), Is.False);
        }

        [Test]
        public void RegisterAssemblyModulesOfBaseType()
        {
            var assembly = typeof(AComponent).Assembly;
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(ModuleBase), assembly);
            var container = builder.Build();

            Assert.That(container.IsRegistered<AComponent>(), Is.True);
            Assert.That(container.IsRegistered<BComponent>(), Is.True);
        }

        class ObjectModule : Module
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
