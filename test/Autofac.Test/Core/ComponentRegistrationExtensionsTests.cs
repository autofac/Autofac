using System;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    public class ComponentRegistrationExtensionsTests
    {
        [Fact]
        public void MatchingLifetimeScopeTags_InstancePerDependency()
        {
            var b = new ContainerBuilder();
            b.RegisterType<DivideByZeroException>().InstancePerDependency();
            var c = b.Build();
            Assert.Empty(c.RegistrationFor<DivideByZeroException>().MatchingLifetimeScopeTags());
        }

        [Fact]
        public void MatchingLifetimeScopeTags_InstancePerLifetimeScope()
        {
            var b = new ContainerBuilder();
            b.RegisterType<DivideByZeroException>().InstancePerLifetimeScope();
            var c = b.Build();
            Assert.Empty(c.RegistrationFor<DivideByZeroException>().MatchingLifetimeScopeTags());
        }

        [Fact]
        public void MatchingLifetimeScopeTags_InstancePerMatchingLifetimeScope_Multiple()
        {
            var b = new ContainerBuilder();
            b.RegisterType<DivideByZeroException>().InstancePerMatchingLifetimeScope("tag1", "tag2");
            var c = b.Build();
            Assert.Equal(new object[] { "tag1", "tag2" }, c.RegistrationFor<DivideByZeroException>().MatchingLifetimeScopeTags());
        }

        [Fact]
        public void MatchingLifetimeScopeTags_InstancePerMatchingLifetimeScope_Single()
        {
            var b = new ContainerBuilder();
            b.RegisterType<DivideByZeroException>().InstancePerMatchingLifetimeScope("tag1");
            var c = b.Build();
            Assert.Equal(new object[] { "tag1" }, c.RegistrationFor<DivideByZeroException>().MatchingLifetimeScopeTags());
        }

        [Fact]
        public void MatchingLifetimeScopeTags_NullRegistration()
        {
            Assert.Throws<ArgumentNullException>(() => ComponentRegistrationExtensions.MatchingLifetimeScopeTags(null));
        }

        [Fact]
        public void MatchingLifetimeScopeTags_Singleton()
        {
            var b = new ContainerBuilder();
            b.RegisterType<DivideByZeroException>().SingleInstance();
            var c = b.Build();
            Assert.Empty(c.RegistrationFor<DivideByZeroException>().MatchingLifetimeScopeTags());
        }
    }
}
