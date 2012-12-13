using System;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class RequestLifetimeScopeProviderFixture
    {
        [Test]
        public void ContainerMustBeProvided()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new RequestLifetimeScopeProvider(null));
            Assert.That(exception.ParamName, Is.EqualTo("container"));
        }

        [Test]
        public void MeaningfulExceptionThrowWhenHttpContextNotAvailable()
        {
            var container = new ContainerBuilder().Build();
            var provider = new RequestLifetimeScopeProvider(container);
            var exception = Assert.Throws<InvalidOperationException>(() => provider.GetLifetimeScope(b => { }));
            Assert.That(exception.Message, Is.EqualTo(RequestLifetimeScopeProviderResources.HttpContextNotAvailable));
        }

        [Test]
        public void ProviderRegisteredWithHttpModule()
        {
            var container = new ContainerBuilder().Build();
            var provider = new RequestLifetimeScopeProvider(container);
            Assert.That(RequestLifetimeHttpModule.LifetimeScopeProvider, Is.EqualTo(provider));
        }
    }
}
