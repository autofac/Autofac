using System;
using System.Reflection;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class FilterKeyFixture
    {
        const FilterScope Scope = FilterScope.Controller;
        readonly Type _controllerType = typeof(TestController);
        readonly MethodInfo _methodInfo = TestController.GetAction1MethodInfo();

        [Test]
        public void ConstructorThrowsIfControllerTypeIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new FilterKey(null, Scope, _methodInfo));

            Assert.That(exception.ParamName, Is.EqualTo("controllerType"));
        }

        [Test]
        public void PropertiesExposeConstructorParameters()
        {
            var key = new FilterKey(_controllerType, Scope, _methodInfo);

            Assert.That(key.ControllerType, Is.EqualTo(_controllerType));
            Assert.That(key.FilterScope, Is.EqualTo(Scope));
            Assert.That(key.MethodInfo, Is.EqualTo(_methodInfo));
        }

        [Test]
        public void EqualsWithMethodInfo()
        {
            var key1 = new FilterKey(_controllerType, Scope, _methodInfo);
            object key2 = new FilterKey(_controllerType, Scope, _methodInfo);

            Assert.That(key1.Equals(key2), Is.True);
        }

        [Test]
        public void EqualsWithNullMethodInfo()
        {
            var key1 = new FilterKey(_controllerType, Scope, null);
            object key2 = new FilterKey(_controllerType, Scope, null);

            Assert.That(key1.Equals(key2), Is.True);
        }

        [Test]
        public void EquatableWithMethodInfo()
        {
            var key1 = new FilterKey(_controllerType, Scope, _methodInfo);
            var key2 = new FilterKey(_controllerType, Scope, _methodInfo);

            Assert.That(key1.Equals(key2), Is.True);
        }

        [Test]
        public void EquatableWithNullMethodInfo()
        {
            var key1 = new FilterKey(_controllerType, Scope, null);
            var key2 = new FilterKey(_controllerType, Scope, null);

            Assert.That(key1.Equals(key2), Is.True);
        }

        [Test]
        public void GetHashCodeWithMethodInfo()
        {
            var key1 = new FilterKey(_controllerType, Scope, _methodInfo);
            var key2 = new FilterKey(_controllerType, Scope, _methodInfo);

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
        }

        [Test]
        public void GetHashCodeWithNullMethodInfo()
        {
            var key1 = new FilterKey(_controllerType, Scope, null);
            var key2 = new FilterKey(_controllerType, Scope, null);

            Assert.That(key1.GetHashCode(), Is.EqualTo(key2.GetHashCode()));
        }
    }
}