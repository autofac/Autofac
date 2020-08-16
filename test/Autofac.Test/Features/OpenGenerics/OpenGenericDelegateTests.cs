using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Autofac.Test.Features.OpenGenerics
{
    public class OpenGenericDelegateTests
    {
        private interface IInterfaceA<T>
        {
        }

        private class ImplementationA<T> : IInterfaceA<T>
        {
        }

        [Fact]
        public void CanResolveByGenericInterface()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric((ctxt, types) => Activator.CreateInstance(typeof(ImplementationA<>).MakeGenericType(types)))
                   .As(typeof(IInterfaceA<>));

            var container = builder.Build();

            var instance = container.Resolve<IInterfaceA<string>>();

            var implementedType = instance.GetType().GetGenericTypeDefinition();

            Assert.Equal(typeof(ImplementationA<>), implementedType);
        }
    }
}
