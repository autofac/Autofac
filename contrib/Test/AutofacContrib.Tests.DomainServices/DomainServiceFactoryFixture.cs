using Autofac;
using AutofacContrib.DomainServices;
using NUnit.Framework;

namespace AutofacContrib.Tests.DomainServices
{
    [TestFixture]
    public class DomainServiceFactoryFixture
    {
        [Test]
        public void FactoryCanCreateDomainService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeDomainService>();

            var factory = new AutofacDomainServiceFactory(builder.Build());
            var service = factory.CreateDomainService(typeof(FakeDomainService), null);

            Assert.IsInstanceOfType(typeof(FakeDomainService), service);
        }

        [Test]
        public void FactoryCanDisposeDomainService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeDomainService>();
            FakeDomainService service;
            using (var container = builder.Build())
            {
                var factory = new AutofacDomainServiceFactory(container);
                service = (FakeDomainService)factory.CreateDomainService(typeof(FakeDomainService), null);
                factory.ReleaseDomainService(service);
                Assert.IsFalse(service.IsDisposed);
            }

            Assert.IsTrue(service.IsDisposed);
        }
    }
}
