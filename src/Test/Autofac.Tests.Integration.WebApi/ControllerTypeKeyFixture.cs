using Autofac.Integration.WebApi;
using NUnit.Framework;

namespace Autofac.Tests.Integration.WebApi
{
    [TestFixture]
    public class ControllerTypeKeyFixture
    {
        [Test]
        public void DerivedTypeDoesNotEqualBaseType()
        {
            var baseKey = new ControllerTypeKey(typeof(TestController));
            var derivedKey = new ControllerTypeKey(typeof(TestControllerA));

            Assert.That(derivedKey, Is.Not.EqualTo(baseKey));
        }

        [Test]
        public void BaseTypeEqualsDerivedType()
        {
            var baseKey = new ControllerTypeKey(typeof(TestController));
            var derivedKey = new ControllerTypeKey(typeof(TestControllerA));

            Assert.That(baseKey, Is.EqualTo(derivedKey));
        }
    }
}