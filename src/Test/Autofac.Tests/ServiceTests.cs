using System;
using NUnit.Framework;
using Autofac.Core;

namespace Autofac.Tests
{
    [TestFixture]
    public class ServiceTests
    {
        [Test]
        public void NameEqualsName()
        {
            Assert.IsTrue(new NamedService("name").Equals(new NamedService("name")));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NameNotNull()
        {
            new NamedService(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void NameNotEmpty()
        {
            new NamedService("");
        }

        [Test]
        public void NameNotEqualsName()
        {
            Assert.IsFalse(new NamedService("name").Equals(new NamedService("another")));
        }

        [Test]
        public void NameNotEqualsType()
        {
            Assert.IsFalse(new NamedService("name").Equals(new TypedService(typeof(object))));
        }

        [Test]
        public void NameNotEqualsNull()
        {
            Assert.IsFalse(new NamedService("name").Equals(null));
        }

        [Test]
        public void TypeEqualsType()
        {
            Assert.IsTrue(new TypedService(typeof(object)).Equals(new TypedService(typeof(object))));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TypeNotNull()
        {
            new TypedService(null);
        }

        [Test]
        public void TypeNotEqualsType()
        {
            Assert.IsFalse(new TypedService(typeof(object)).Equals(new TypedService(typeof(string))));
        }

        [Test]
        public void TypeNotEqualsName()
        {
            Assert.IsFalse(new TypedService(typeof(object)).Equals(new NamedService("name")));
        }

        [Test]
        public void TypeNotEqualsNull()
        {
            Assert.IsFalse(new TypedService(typeof(object)).Equals(null));
        }


    }
}
