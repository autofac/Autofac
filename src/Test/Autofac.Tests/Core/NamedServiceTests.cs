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
            var name = "name";
            var type = typeof(object);
            Assert.IsTrue(new NamedService(name, type).Equals(new NamedService(name, type)));
        }

        [Test]
        public void ContructorRequires_NameNotNull()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new NamedService(null, typeof(object));
            });
        }

        [Test]
        public void ContructorRequires_TypeNotNull()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new NamedService("name", null);
            });
        }

        [Test]
        public void ContructorRequires_NameNotEmpty()
        {
            Assertions.AssertThrows<ArgumentException>(delegate
            {
                new NamedService(String.Empty, typeof(object));
            });
        }

        [Test]
        public void NamedServicesForDifferentNames_AreNotEqual()
        {
            Assert.IsFalse(new NamedService("name", typeof(object)).Equals(
                new NamedService("another", typeof(object))));
        }

        [Test]
        public void NamedServicesForDifferentTypes_AreNotEqual()
        {
            Assert.IsFalse(new NamedService("name", typeof(object)).Equals(
                new NamedService("name", typeof(string))));
        }

        [Test]
        public void NamedServices_AreNotEqualToOtherServiceTypes()
        {
            Assert.IsFalse(new NamedService("name", typeof(object)).Equals(new TypedService(typeof(object))));
        }

        [Test]
        public void ANamedService_IsNotEqualToNull()
        {
            Assert.IsFalse(new NamedService("name", typeof(object)).Equals(null));
        }

        [Test]
        public void ChangeType_ProvidesNamedServiceWithNewTypeAndSameName()
        {
            var nt = typeof(string);
            var name = "name";
            var ns = new NamedService(name, typeof(object));
            var n = ns.ChangeType(nt);
            Assert.AreEqual(new NamedService(name, nt), n);
        }
    }
}
