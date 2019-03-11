using System;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class ModuleRegistration
    {
        [Fact]
        public void ModuleCanRegisterModule()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<Module2>();
            var container = builder.Build();
            Assert.NotNull(container.Resolve<object>());
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
    }
}
