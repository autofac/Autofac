using System;
using System.Linq;
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

        [Fact]
        public void WhenAnExceptionIsThrownFromAConstructor_TheInnerExceptionIsWrapped()
        {
            var ci = typeof (ThrowsInCtor).GetConstructor(new Type[0]);
            var cpb = new ConstructorParameterBinding(
                ci, Enumerable.Empty<Parameter>(), new ContainerBuilder().Build());
            var dx = Assert.Throws<DependencyResolutionException>(() =>
                cpb.Instantiate());

            Assert.True(dx.Message.Contains(typeof(ThrowsInCtor).Name));
            Assert.Equal(ThrowsInCtor.Message, dx.InnerException.Message);
        }
    }
}
