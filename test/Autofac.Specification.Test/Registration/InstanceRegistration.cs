using System;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class InstanceRegistration
    {
        private interface IA
        {
        }

        [Fact]
        public void ProvidedInstancesCannotSupportInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerDependency();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void RegisterInstanceAsUnsupportedService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance("hello").As<IA>();
            Assert.Throws<ArgumentException>(() => builder.Build());
        }
    }
}
