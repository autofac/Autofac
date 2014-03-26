using System;
using Autofac.Integration.Owin;
using Microsoft.Owin;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Owin
{
    [TestFixture]
    public class OwinContextExtensionsFixture
    {
        [Test]
        public void GetAutofacLifetimeScopeReturnsInstanceFromContext()
        {
            var context = new Mock<IOwinContext>();
            context.Setup(mock => mock.Get<ILifetimeScope>(Constants.OwinLifetimeScopeKey));

            context.Object.GetAutofacLifetimeScope();

            context.VerifyAll();
        }

        [Test]
        public void GetAutofacLifetimeScopeThrowsWhenProvidedNullInstance()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => OwinContextExtensions.GetAutofacLifetimeScope(null));
            Assert.That(exception.ParamName, Is.EqualTo("context"));
        }
    }
}
