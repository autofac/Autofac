using System;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using NUnit.Framework;

namespace Autofac.Tests.Core.Activators.Reflection
{
    [TestFixture]
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

        [Test]
        public void WhenAnExceptionIsThrownFromAConstructor_TheInnerExceptionIsWrapped()
        {
            var ci = typeof (ThrowsInCtor).GetConstructor(new Type[0]);
            var cpb = new ConstructorParameterBinding(
                ci, Enumerable.Empty<Parameter>(), Container.Empty);
            var dx = Assert.Throws<DependencyResolutionException>(() =>
                cpb.Instantiate());

            Assert.That(dx.Message.Contains(typeof(ThrowsInCtor).Name));
            Assert.AreEqual(ThrowsInCtor.Message, dx.InnerException.Message);
        }
    }
}
