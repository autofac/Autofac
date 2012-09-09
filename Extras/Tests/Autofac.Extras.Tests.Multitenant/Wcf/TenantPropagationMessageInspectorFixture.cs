using System;
using System.ServiceModel.Channels;
using Autofac.Extras.Multitenant.Wcf;
using Autofac.Extras.Tests.Multitenant.Stubs;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant.Wcf
{
    [TestFixture]
    public class TenantPropagationMessageInspectorFixture
    {
        [Test(Description = "Ensures you must provide a function to resolve the container provider.")]
        public void Ctor_NullContainerProviderFunction()
        {
            Assert.Throws<ArgumentNullException>(() => new TenantPropagationMessageInspector<string>(null));
        }

        [Test(Description = "Checks that AfterReceiveReply is basically a no-op.")]
        public void AfterReceiveReply_NoOp()
        {
            var inspector = new TenantPropagationMessageInspector<string>(new StubTenantIdentificationStrategy());
            Message msg = null;
            Assert.DoesNotThrow(() => inspector.AfterReceiveReply(ref msg, null));
        }

        [Test(Description = "Checks that BeforeSendReply is basically a no-op.")]
        public void BeforeSendReply_NoOp()
        {
            var inspector = new TenantPropagationMessageInspector<string>(new StubTenantIdentificationStrategy());
            Message msg = null;
            Assert.DoesNotThrow(() => inspector.BeforeSendReply(ref msg, null));
        }
    }
}
