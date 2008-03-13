using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Component;

namespace Autofac.Tests.Component
{
    [TestFixture]
    public class DescriptorFixture
    {
        [Test]
        public void AcceptsConcreteImplType()
        {
            var target = CreateWithImplementationType(typeof(object));
            Type impl;
            Assert.IsTrue(target.KnownImplementationType(out impl));
            Assert.AreEqual(typeof(object), impl);
        }

        [Test]
        public void DiscardsInterfaceImplType()
        {
            var target = CreateWithImplementationType(typeof(IDisposable));
            Type impl;
            Assert.IsFalse(target.KnownImplementationType(out impl));
            Assert.IsNull(impl);
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
