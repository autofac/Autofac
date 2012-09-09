using System.ServiceModel.DomainServices.Server;
using Moq;
using NUnit.Framework;
using Autofac.Extras.DomainServices;
using Autofac;
using System;
using System.Reflection;
namespace Autofac.Extras.Tests.DomainServices
{
    [TestFixture]
    public class DomainServiceModuleFixture
    {

        [Test]
        public void ModuleInitalizeDomainServices()
        {
            var builder = new ContainerBuilder();
            var domainServiceMock = new Mock<DomainService>();
            builder.RegisterInstance(domainServiceMock.Object).As<DomainService>();
            builder.RegisterModule<AutofacDomainServiceModule>();
            using (var container = builder.Build())
            {
                var service = container.Resolve<DomainService>(TypedParameter.From(domainServiceContext));
                Assert.IsNotNull(service);
                domainServiceMock.Verify(ds => ds.Initialize(domainServiceContext));
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
