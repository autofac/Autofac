using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Core.Activators.Reflection
{
    public class ConstructorParameterBindingTests
    {
        public class ThrowsInCtor
        {
            public const string Message = "Message";

            public ThrowsInCtor()
            {
                throw new InvalidOperationException(Message);
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
            var cpb = new ConstructorParameterBinding(
                ci, Enumerable.Empty<Parameter>(), new ContainerBuilder().Build());
            var dx = Assert.Throws<DependencyResolutionException>(() =>
                cpb.Instantiate());

            Assert.True(dx.Message.Contains(typeof(ThrowsInCtor).Name));
            Assert.Equal(ThrowsInCtor.Message, dx.InnerException.Message);
        }

        [Fact]
        public void WhenPrimitiveTypeIsProvidedForPrimitiveParameterConversionWillBeAttempted()
        {
            var ci = typeof(CtorWithDoubleParam).GetTypeInfo().DeclaredConstructors.Single();
            var cpb = new ConstructorParameterBinding(
                ci, new[] { new PositionalParameter(0, 1), }, new ContainerBuilder().Build());
            var instance = (CtorWithDoubleParam)cpb.Instantiate();

            Assert.Equal(1d, instance.Value);
        }

        [Fact]
        public void WhenEnumTypeIsProvidedForIntParameterConversionWillBeAttempted()
        {
            var ci = typeof(CtorWithInt).GetTypeInfo().DeclaredConstructors.Single();
            var cpb = new ConstructorParameterBinding(
                ci, new[] { new PositionalParameter(0, Foo.B), }, new ContainerBuilder().Build());
            var instance = (CtorWithInt)cpb.Instantiate();

            Assert.Equal(1, instance.Value);
        }
    }
}
