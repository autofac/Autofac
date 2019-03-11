using System;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class OpenGenericTests
    {
        private interface IImplementedInterface<T>
        {
        }

        [Fact]
        public void AsImplementedInterfacesOnOpenGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(SelfComponent<>)).AsImplementedInterfaces();
            var context = builder.Build();
            context.Resolve<IImplementedInterface<object>>();
        }

        [Fact]
        public void AsSelfOnOpenGeneric()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(SelfComponent<>)).AsSelf();
            var context = builder.Build();
            context.Resolve<SelfComponent<object>>();
        }

        private class SelfComponent<T> : IImplementedInterface<T>
        {
        }
    }
}
