using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Autofac.Extras.Multitenant.Wcf;
using Autofac.Extras.Tests.Multitenant.Stubs;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant.Wcf
{
    [TestFixture]
    public class TenantPropagationBehaviorFixture
    {
        [Test(Description = "Ensures you must provide a function to resolve the container provider.")]
        public void Ctor_NullContainerProviderFunction()
        {
            Assert.Throws<ArgumentNullException>(() => new TenantPropagationBehavior<string>(null));
        }

        [Test(Description = "Checks that AddBindingParameters is basically a no-op.")]
        public void AddBindingParameters2_NoOp()
        {
            var behavior = new TenantPropagationBehavior<string>(new StubTenantIdentificationStrategy());
            Assert.DoesNotThrow(() => behavior.AddBindingParameters(null, null));
        }

        [Test(Description = "Checks that AddBindingParameters is basically a no-op.")]
        public void AddBindingParameters4_NoOp()
        {
            var behavior = new TenantPropagationBehavior<string>(new StubTenantIdentificationStrategy());
            Assert.DoesNotThrow(() => behavior.AddBindingParameters(null, null, null, null));
        }

        [Test(Description = "Checks that ApplyClientBehavior validates the client runtime.")]
        public void ApplyClientBehavior_NullServiceHost()
        {
            var behavior = new TenantPropagationBehavior<string>(new StubTenantIdentificationStrategy());
            var ex = Assert.Throws<ArgumentNullException>(() => behavior.ApplyClientBehavior(null, null));
            Assert.AreEqual("clientRuntime", ex.ParamName, "The wrong parameter was validated.");
        }

        [Test(Description = "Checks that ApplyDispatchBehavior for a client is basically a no-op.")]
        public void ApplyDispatchBehavior_Client_NoOp()
        {
            var behavior = new TenantPropagationBehavior<string>(new StubTenantIdentificationStrategy());
            Assert.DoesNotThrow(() => behavior.ApplyDispatchBehavior((ServiceEndpoint)null, (EndpointDispatcher)null));
        }

        [Test(Description = "Checks that ApplyDispatchBehavior for a service validates the incoming service host.")]
        public void ApplyDispatchBehavior_Service_NullServiceHost()
        {
            var behavior = new TenantPropagationBehavior<string>(new StubTenantIdentificationStrategy());
            var ex = Assert.Throws<ArgumentNullException>(() => behavior.ApplyDispatchBehavior((ServiceDescription)null, (ServiceHostBase)null));
            Assert.AreEqual("serviceHostBase", ex.ParamName, "The wrong parameter was validated.");
        }

        [Test(Description = "Checks that Validate is basically a no-op.")]
        public void Validate1_NoOp()
        {
            var behavior = new TenantPropagationBehavior<string>(new StubTenantIdentificationStrategy());
            Assert.DoesNotThrow(() => behavior.Validate(null));
        }

        [Test(Description = "Checks that Validate is basically a no-op.")]
        public void Validate2_NoOp()
        {
            var behavior = new TenantPropagationBehavior<string>(new StubTenantIdentificationStrategy());
            Assert.DoesNotThrow(() => behavior.Validate(null, null));
        }
    }
}
