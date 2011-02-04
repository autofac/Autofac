using System;
using Autofac.Integration.Mvc;
using NUnit.Framework;

namespace Autofac.Tests.Integration.Mvc
{
    [TestFixture]
    public class RequestLifetimeHttpModuleFixture
    {
        [Test]
        public void MeaningfulExceptionThrowWhenHttpContextNotAvailable()
        {
            var container = new ContainerBuilder().Build();
            var exception = Assert.Throws<InvalidOperationException>(() => RequestLifetimeHttpModule.GetLifetimeScope(container, null));
            Assert.That(exception.Message, Is.EqualTo(RequestLifetimeHttpModuleResources.HttpContextNotAvailable));
        }
    }
}
