using System;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class KeyedServiceTests
    {
        [Test]
        public void KeyedServicesForTheSameName_AreEqual()
        {
            var key = new object();
            var type = typeof(object);
            Assert.IsTrue(new KeyedService(key, type).Equals(new KeyedService(key, type)));
        }

        [Test]
        public void ContructorRequires_KeyNotNull()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new KeyedService(null, typeof(object));
            });
        }

        [Test]
        public void ContructorRequires_TypeNotNull()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new KeyedService("name", null);
            });
        }

        [Test]
        public void KeyedServicesForDifferentKeys_AreNotEqual()
        {
            var key1 = new object();
            var key2 = new object();

            Assert.IsFalse(new KeyedService(key1, typeof(object)).Equals(
                new KeyedService(key2, typeof(object))));
        }

        [Test]
        public void KeyedServicesForDifferentTypes_AreNotEqual()
        {
            var key = new object();

            Assert.IsFalse(new KeyedService(key, typeof(object)).Equals(
                new KeyedService(key, typeof(string))));
        }

        [Test]
        public void KeyedServices_AreNotEqualToOtherServiceTypes()
        {
            Assert.IsFalse(new KeyedService(new object(), typeof(object)).Equals(new TypedService(typeof(object))));
        }

        [Test]
        public void AKeyedService_IsNotEqualToNull()
        {
            Assert.IsFalse(new KeyedService(new object(), typeof(object)).Equals(null));
        }

        [Test]
        public void ChangeType_ProvidesKeyedServiceWithNewTypeAndSameKey()
        {
            var newType = typeof(string);
            var key = new object();
            var service = new KeyedService(key, typeof(object));
            var changedService = service.ChangeType(newType);
            Assert.AreEqual(new KeyedService(key, newType), changedService);
        }
    }
}
