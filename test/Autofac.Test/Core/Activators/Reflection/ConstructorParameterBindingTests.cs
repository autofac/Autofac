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
    }
}
