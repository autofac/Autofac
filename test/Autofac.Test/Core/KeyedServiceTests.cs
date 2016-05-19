using System;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    public class KeyedServiceTests
    {
        [Fact]
        public void KeyedServicesForTheSameName_AreEqual()
        {
            var key = new object();
            var type = typeof(object);
            Assert.True(new KeyedService(key, type).Equals(new KeyedService(key, type)));
        }

        [Fact]
        public void ContructorRequires_KeyNotNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new KeyedService(null, typeof(object));
            });
        }

        [Fact]
        public void ContructorRequires_TypeNotNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new KeyedService("name", null);
            });
        }

        [Fact]
        public void KeyedServicesForDifferentKeys_AreNotEqual()
        {
            var key1 = new object();
            var key2 = new object();

            Assert.False(new KeyedService(key1, typeof(object)).Equals(
                new KeyedService(key2, typeof(object))));
        }

        [Fact]
        public void KeyedServicesForDifferentTypes_AreNotEqual()
        {
            var key = new object();

            Assert.False(new KeyedService(key, typeof(object)).Equals(
                new KeyedService(key, typeof(string))));
        }

        [Fact]
        public void KeyedServices_AreNotEqualToOtherServiceTypes()
        {
            Assert.False(new KeyedService(new object(), typeof(object)).Equals(new TypedService(typeof(object))));
        }

        [Fact]
        public void AKeyedService_IsNotEqualToNull()
        {
            Assert.False(new KeyedService(new object(), typeof(object)).Equals(null));
        }

        [Fact]
        public void ChangeType_ProvidesKeyedServiceWithNewTypeAndSameKey()
        {
            var newType = typeof(string);
            var key = new object();
            var service = new KeyedService(key, typeof(object));
            var changedService = service.ChangeType(newType);
            Assert.Equal(new KeyedService(key, newType), changedService);
        }
    }
}
