using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class TypedServiceTests
    {
        [Test]
        public void TypedServicesForTheSameType_AreEqual()
        {
            Assert.IsTrue(new TypedService(typeof(object)).Equals(new TypedService(typeof(object))));
        }

        [Test]
        public void ConstructorRequires_TypeNotNull()
        {
            Assertions.AssertThrows<ArgumentNullException>(delegate
            {
                new TypedService(null);
            });
        }

        [Test]
        public void TypedServicesForDifferentTypes_AreNotEqual()
        {
            Assert.IsFalse(new TypedService(typeof(object)).Equals(new TypedService(typeof(string))));
        }

        [Test]
        public void TypedServices_DoNotEqualOtherServiceTypes()
        {
            Assert.IsFalse(new TypedService(typeof(object)).Equals(new NamedService("name")));
        }

        [Test]
        public void ATypedService_IsNotEqualToNull()
        {
            Assert.IsFalse(new TypedService(typeof(object)).Equals(null));
        }
    }
}
