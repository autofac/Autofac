using System.ServiceModel.DomainServices.Server;
using NUnit.Framework;
using AutofacContrib.DomainServices;
using Autofac;
using System;
using System.Reflection;
namespace AutofacContrib.Tests.DomainServices
{
    [TestFixture]
    public class DomainServiceModuleFixture
    {

        [Test]
        public void ModuleRegistersDomainServicesInSuppliedAssemblies()
        {
            var builder = new ContainerBuilder();

            var module = new AutofacDomainServiceModule(Assembly.GetExecutingAssembly());
            builder.RegisterModule(module);
            using (var container = builder.Build())
            {
                var service = container.Resolve<FakeDomainService>(TypedParameter.From(domainServiceContext));
                Assert.IsNotNull(service);
            }
        }

        [Test]
        public void ModuleRegistersDomainServicesInCurrentAssembly()
        {
            var builder = new ContainerBuilder();

            var module = new AutofacDomainServiceModule();
            builder.RegisterModule(module);
            using (var container = builder.Build())
            {
                var service = container.Resolve<FakeDomainService>(TypedParameter.From(domainServiceContext));
                Assert.IsNotNull(service);
            }
        }

        readonly DomainServiceContext domainServiceContext = new DomainServiceContext(new FakeServiceProvider(), DomainOperationType.Invoke);
        class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }
        }
    }
}
