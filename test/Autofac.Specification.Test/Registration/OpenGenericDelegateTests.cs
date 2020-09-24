// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Registration;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class OpenGenericDelegateTests
    {
        private interface IInterfaceA<T>
        {
        }

        private interface IInterfaceB<T>
        {
        }

        private interface IInterfaceMultiType<T1, T2>
        {
        }

        private class ImplementationA<T> : IInterfaceA<T>
        {
        }

        private class ImplementationMultiType<T1, T2> : IInterfaceMultiType<T1, T2>
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

        [Fact]
        public void DoesNotResolveForDifferentGenericService()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric((ctxt, types) => Activator.CreateInstance(typeof(ImplementationA<>).MakeGenericType(types)))
                   .As(typeof(IInterfaceA<>));

            var container = builder.Build();

            Assert.Throws<ComponentNotRegisteredException>(() => container.Resolve<IInterfaceB<string>>());
        }

        [Fact]
        public void MultipleTypeArgsSupported()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric((ctxt, types) => Activator.CreateInstance(typeof(ImplementationMultiType<,>).MakeGenericType(types)))
                   .As(typeof(IInterfaceMultiType<,>));

            var container = builder.Build();

            var instance = container.Resolve<IInterfaceMultiType<string, int>>();

            var implementedType = instance.GetType().GetGenericTypeDefinition();

            Assert.Equal(typeof(ImplementationMultiType<,>), implementedType);
        }

        [Fact]
        public void VariableTypeArgsLengthSupported()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric((ctxt, types) =>
            {
                var chosenType = types.Length == 2 ? typeof(ImplementationMultiType<,>) : typeof(ImplementationA<>);

                return Activator.CreateInstance(chosenType.MakeGenericType(types));
            })
            .As(typeof(IInterfaceMultiType<,>))
            .As(typeof(IInterfaceA<>));

            var container = builder.Build();

            var instance = container.Resolve<IInterfaceA<string>>();
            var implementedType = instance.GetType().GetGenericTypeDefinition();
            Assert.Equal(typeof(ImplementationA<>), implementedType);

            var multiInstance = container.Resolve<IInterfaceMultiType<string, int>>();
            implementedType = multiInstance.GetType().GetGenericTypeDefinition();
            Assert.Equal(typeof(ImplementationMultiType<,>), implementedType);
        }

        [Fact]
        public void GenericDelegateCanReceiveParameters()
        {
            var builder = new ContainerBuilder();

            List<Parameter> passedParameters = null;

            builder.RegisterGeneric((ctxt, types, parameters) =>
            {
                passedParameters = parameters.ToList();

                return Activator.CreateInstance(typeof(ImplementationA<>).MakeGenericType(types));
            })
            .As(typeof(IInterfaceA<>));

            var container = builder.Build();

            var instance = container.Resolve<IInterfaceA<int>>(new TypedParameter(typeof(bool), true));

            Assert.Collection(
                passedParameters,
                p => Assert.IsType<TypedParameter>(p));
        }

        [Fact]
        public void CastExceptionIfDelegateReturnsBadObject()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric((ctxt, types) => "bad")
                   .As(typeof(IInterfaceA<>));

            var container = builder.Build();

            var innerException = Assert.Throws<DependencyResolutionException>(() => container.Resolve<IInterfaceA<string>>()).InnerException;

            Assert.IsType<InvalidCastException>(innerException);
        }

        [Fact]
        public void ExceptionThrownByDelegateIsWrapped()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric((ctxt, types) => throw new DivideByZeroException())
                   .As(typeof(IInterfaceA<>));

            var container = builder.Build();

            var innerException = Assert.Throws<DependencyResolutionException>(() => container.Resolve<IInterfaceA<int>>()).InnerException;

            Assert.IsType<DivideByZeroException>(innerException);
        }
    }
}
