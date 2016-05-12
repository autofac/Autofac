using System;
using Autofac.Builder;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Builder
{
    public class ProvidedInstanceRegistrationBuilderTests
    {
        [Fact]
        public void NullCannotBeRegisteredAsAnInstance()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.RegisterInstance((object)null));
        }

        [Fact]
        public void InstancePerDependency_NotValidForProvidedInstances()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerDependency();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void InstancePerLifetimeScope_NotValidForProvidedInstances()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new object()).InstancePerLifetimeScope();
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void LimitType_ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterInstance("Hello").As<object>();
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.True(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(object)), out cr));
            Assert.Equal(typeof(string), cr.Activator.LimitType);
        }

        [Fact]
        public void DefaultServiceType_IsStaticTypeOfRegisteredInstance()
        {
            object instance = "Hello";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance);
            var container = builder.Build();
            container.AssertRegistered<object>();
            container.AssertNotRegistered<string>();
        }
    }
}
