using System;
using Autofac.Component;
using NUnit.Framework;

namespace Autofac.Tests.Component
{
    [TestFixture]
    public class DescriptorFixture
    {
        [Test]
        public void AcceptsConcreteImplType()
        {
            var target = CreateWithImplementationType(typeof(object));
            Assert.AreEqual(typeof(object), target.BestKnownImplementationType);
        }

        [Test]
        public void AllowsInterfaceImplType()
        {
            var target = CreateWithImplementationType(typeof(IDisposable));
            Assert.AreEqual(typeof(IDisposable), target.BestKnownImplementationType);
        }

        private Descriptor CreateWithImplementationType(Type type)
        {
            return new Descriptor(
                new UniqueService(),
                new Service[0],
                type);
        }
    }
}
