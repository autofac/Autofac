using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    public class TypedServiceTests
    {
        [Fact]
        public void TypedServicesForTheSameType_AreEqual()
        {
            Assert.True(new TypedService(typeof(object)).Equals(new TypedService(typeof(object))));
        }

        [Fact]
        public void ConstructorRequires_TypeNotNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                new TypedService(null);
            });
        }

        [Fact]
        public void TypedServicesForDifferentTypes_AreNotEqual()
        {
            Assert.False(new TypedService(typeof(object)).Equals(new TypedService(typeof(string))));
        }

        [Fact]
        public void TypedServices_DoNotEqualOtherServiceTypes()
        {
            Assert.False(new TypedService(typeof(object)).Equals(new KeyedService("name", typeof(object))));
        }

        [Fact]
        public void ATypedService_IsNotEqualToNull()
        {
            Assert.False(new TypedService(typeof(object)).Equals(null));
        }

        [Fact]
        public void ChangeType_ProvidesTypedServiceWithNewType()
        {
            var nt = typeof(string);
            var ts = new TypedService(typeof(object));
            var n = ts.ChangeType(nt);
            Assert.Equal(new TypedService(nt), n);
        }
    }
}
