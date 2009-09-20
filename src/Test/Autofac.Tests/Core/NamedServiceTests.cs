using System;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class NamedServiceTests
    {
        [Test]
        public void NamedServicesForTheSameName_AreEqual()
        {
            Assert.IsTrue(new NamedService("name").Equals(new NamedService("name")));
        }

        [Test]
        public void ContructorRequires_NameNotNull()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new NamedService(null);
            });
        }

        [Test]
        public void ContructorRequires_NameNotEmpty()
        {
            Assertions.AssertThrows<ArgumentException>(delegate
            {
                new NamedService(String.Empty);
            });
        }

        [Test]
        public void NamedServicesForDifferentNames_AreNotEqual()
        {
            Assert.IsFalse(new NamedService("name").Equals(new NamedService("another")));
        }

        [Test]
        public void NamedServices_AreNotEqualToOtherServiceTypes()
        {
            Assert.IsFalse(new NamedService("name").Equals(new TypedService(typeof(object))));
        }

        [Test]
        public void ANamedService_IsNotEqualToNull()
        {
            Assert.IsFalse(new NamedService("name").Equals(null));
        }
    }
}
