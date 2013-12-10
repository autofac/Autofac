using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core.Registration;
using NUnit.Framework;

namespace Autofac.Tests.Core.Registration
{
    [TestFixture]
    public class ModuleRegistrarTests
    {
        [Test]
        public void Ctor_RequiresContainerBuilder()
        {
            Assert.Throws<ArgumentNullException>(() => new ModuleRegistrar(null));
        }

        [Test]
        public void RegisterModule_ChainsModuleRegistrations()
        {
            var builder = new ContainerBuilder();
            var registrar = new ModuleRegistrar(builder);
            registrar.RegisterModule(new ModuleA()).RegisterModule(new ModuleB());
            var container = builder.Build();
            var strings = container.Resolve<IEnumerable<string>>();
            Assert.IsTrue(strings.Contains("foo"));
            Assert.IsTrue(strings.Contains("bar"));
        }

        [Test]
        public void RegisterModule_RequiresModule()
        {
            var registrar = new ModuleRegistrar(new ContainerBuilder());
            Assert.Throws<ArgumentNullException>(() => registrar.RegisterModule(null));
        }

        private class ModuleA : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterInstance("foo").As<string>();
            }
        }

        private class ModuleB : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterInstance("bar").As<string>();
            }
        }
    }
}
