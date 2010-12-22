using System;
using Autofac.Tests.Scenarios.ScannedAssembly;
using NUnit.Framework;

namespace Autofac.Tests
{
    [TestFixture]
    public class TypeExtensionsTests
    {
        [Test]
        public void IsClosedTypeOfNonGenericTypeProvidedThrowsException()
        {
            Assert.Throws<ArgumentException>(() => 
                typeof(object).IsClosedTypeOf(typeof(SaveCommandData)));
        }

        [Test]
        public void IsClosedTypeOfClosedGenericTypeProvidedThrowsException()
        {
            var cb = new ContainerBuilder();
            Assert.Throws<ArgumentException>(() =>
                typeof(object).IsClosedTypeOf(typeof(ICommand<SaveCommandData>)));
        }

    }
}
