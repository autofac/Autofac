using System;
using Autofac.Integration.Mvc;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class RequestLifetimeHttpModuleFixture
    {
        [Test]
        public void CannotSetNullLifetimeScopeProvider()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => RequestLifetimeHttpModule.SetLifetimeScopeProvider(null));
            Assert.That(exception.ParamName, Is.EqualTo("lifetimeScopeProvider"));
        }

        [Test]
        public void CanSetNonNullLifetimeScopeProvider()
        {
            var provider = new Mock<ILifetimeScopeProvider>();
            RequestLifetimeHttpModule.SetLifetimeScopeProvider(provider.Object);
            Assert.That(RequestLifetimeHttpModule.LifetimeScopeProvider, Is.EqualTo(provider.Object));
        }
    }
}
