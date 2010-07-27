using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac;
using AutofacContrib.Multitenant.Wcf;
using NUnit.Framework;

namespace AutofacContrib.Tests.Multitenant.Wcf
{
    [TestFixture]
    public class AutofacDependencyInjectionServiceBehaviorFixture
    {
        [Test(Description = "Verifies that you must provide a container from which instances will be resolved.")]
        public void Ctor_RequiresContainer()
        {
            var data = new ServiceImplementationData();
            Assert.Throws<ArgumentNullException>(() => new AutofacDependencyInjectionServiceBehavior(null, data));
        }

        [Test(Description = "Verifies that you must provide data about the service instance to resolve.")]
        public void Ctor_RequiresServiceImplementationData()
        {
            var container = new ContainerBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => new AutofacDependencyInjectionServiceBehavior(container, null));
        }

        [Test(Description = "Verifies that the constructor parameters are properly stored for later use.")]
        public void Ctor_StoresParameters()
        {
            var data = new ServiceImplementationData();
            var container = new ContainerBuilder().Build();
            var provider = new AutofacDependencyInjectionServiceBehavior(container, data);
            Assert.AreSame(data, provider.ServiceData, "The service implementation data was not stored.");
            Assert.AreSame(container, provider.Container, "The container was not stored.");
        }

        [Test(Description = "Attempts to apply the behavior without a service description.")]
        public void ApplyDispatchBehavior_NullServiceDescription()
        {
            var provider = new AutofacDependencyInjectionServiceBehavior(new ContainerBuilder().Build(), new ServiceImplementationData());
            var host = new TestHost();
            Assert.Throws<ArgumentNullException>(() => provider.ApplyDispatchBehavior(null, host));
        }

        [Test(Description = "Attempts to apply the behavior without a service description.")]
        public void ApplyDispatchBehavior_NullServiceHost()
        {
            var provider = new AutofacDependencyInjectionServiceBehavior(new ContainerBuilder().Build(), new ServiceImplementationData());
            var description = new ServiceDescription();
            Assert.Throws<ArgumentNullException>(() => provider.ApplyDispatchBehavior(description, null));
        }

        private class TestHost : ServiceHostBase
        {
            protected override System.ServiceModel.Description.ServiceDescription CreateDescription(out IDictionary<string, System.ServiceModel.Description.ContractDescription> implementedContracts)
            {
                throw new NotImplementedException();
            }
        }
    }
}
