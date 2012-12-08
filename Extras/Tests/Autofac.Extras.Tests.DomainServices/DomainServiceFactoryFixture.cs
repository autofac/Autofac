using Autofac;
using Autofac.Extras.DomainServices;
using Autofac.Integration.Web;
using NUnit.Framework;

namespace Autofac.Extras.Tests.DomainServices
{
    [TestFixture]
    public class DomainServiceFactoryFixture
    {
        [Test]
        public void FactoryCanCreateDomainService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeDomainService>();

            var containerProvider = new TestContainerProvider(builder.Build());
            var factory = new AutofacDomainServiceFactory(containerProvider);
            var service = factory.CreateDomainService(typeof(FakeDomainService), null);

            Assert.IsInstanceOf<FakeDomainService>(service);
        }

        [Test]
        public void FactoryCanDisposeDomainService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeDomainService>();
            FakeDomainService service;
            var containerProvider = new TestContainerProvider(builder.Build());
            var factory = new AutofacDomainServiceFactory(containerProvider);
            service = (FakeDomainService)factory.CreateDomainService(typeof(FakeDomainService), null);
            factory.ReleaseDomainService(service);
            Assert.IsFalse(service.IsDisposed);

            containerProvider.EndRequestLifetime();
            Assert.IsTrue(service.IsDisposed);
        }

        private class TestContainerProvider : IContainerProvider
        {
            private ILifetimeScope _request;

            public TestContainerProvider(IContainer container)
            {
                this.ApplicationContainer = container;
            }

            public void EndRequestLifetime()
            {
                if (this._request != null)
                {
                    this._request.Dispose();
                    this._request = null;
                }
            }

            public ILifetimeScope ApplicationContainer { get; private set; }

            public ILifetimeScope RequestLifetime
            {
                get
                {
                    if (this._request == null)
                    {
                        this._request = this.ApplicationContainer.BeginLifetimeScope();
                    }
                    return this._request;
                }
            }
        }
    }
}
