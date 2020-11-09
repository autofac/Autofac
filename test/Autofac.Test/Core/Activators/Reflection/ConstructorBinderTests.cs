// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Core.Activators.Reflection
{
    public class ConstructorBinderTests
    {
        public class ThrowsInCtor
        {
            public const string Message = "Message";

            public ThrowsInCtor()
            {
                throw new InvalidOperationException(Message);
            }
        }

        public class ServiceWithInParameter
        {
            public ServiceWithInParameter(in int input)
            {
            }
        }

        public class CtorWithDoubleParam
        {
            public double Value { get; }

            public CtorWithDoubleParam(double value)
            {
                Value = value;
            }
        }

        public enum Foo
        {
            A,
            B
        }

        public class CtorWithInt
        {
            public int Value { get; }

            public CtorWithInt(int value)
            {
                Value = value;
            }
        }

        [Fact]
        public void WhenAnExceptionIsThrownFromAConstructor_TheInnerExceptionIsWrapped()
        {
            var ci = typeof(ThrowsInCtor).GetTypeInfo().DeclaredConstructors.Single();
            var cpb = new ConstructorBinder(ci).Bind(Enumerable.Empty<Parameter>(), new ContainerBuilder().Build());
            var dx = Assert.Throws<DependencyResolutionException>(() =>
                cpb.Instantiate());

            Assert.Contains(nameof(ThrowsInCtor), dx.Message);
            Assert.NotNull(dx.InnerException);
            Assert.Equal(ThrowsInCtor.Message, dx.InnerException.Message);
        }

        [Fact]
        public void WhenPrimitiveTypeIsProvidedForPrimitiveParameterConversionWillBeAttempted()
        {
            var ci = typeof(CtorWithDoubleParam).GetTypeInfo().DeclaredConstructors.Single();
            var cpb = new ConstructorBinder(ci).Bind(new[] { new PositionalParameter(0, 1), }, new ContainerBuilder().Build());
            var instance = (CtorWithDoubleParam)cpb.Instantiate();

            Assert.Equal(1d, instance.Value);
        }

        [Fact]
        public void WhenEnumTypeIsProvidedForIntParameterConversionWillBeAttempted()
        {
            var ci = typeof(CtorWithInt).GetTypeInfo().DeclaredConstructors.Single();
            var cpb = new ConstructorBinder(ci).Bind(new[] { new PositionalParameter(0, Foo.B), }, new ContainerBuilder().Build());
            var instance = (CtorWithInt)cpb.Instantiate();

            Assert.Equal(1, instance.Value);
        }

        [Fact]
        public void WhenUsingByRefParameterExceptionIsThrownForIllegalParameter() // added for issue 1126
        {
            var constructorInfo = typeof(ServiceWithInParameter).GetTypeInfo().DeclaredConstructors.Single();
            var constructorBinder = new ConstructorBinder(constructorInfo);
            var boundConstructor = constructorBinder.Bind(new[] { new PositionalParameter(0, 111) }, new ContainerBuilder().Build());
            Assert.False(boundConstructor.CanInstantiate);
            Assert.Throws<InvalidOperationException>(() => boundConstructor.Instantiate());
        }
    }
}
