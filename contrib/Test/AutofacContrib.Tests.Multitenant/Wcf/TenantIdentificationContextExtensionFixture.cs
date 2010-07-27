using System;
using AutofacContrib.Multitenant.Wcf;
using NUnit.Framework;

namespace AutofacContrib.Tests.Multitenant.Wcf
{
    [TestFixture]
    public class TenantIdentificationContextExtensionFixture
    {
        [Test(Description = "Verifies that Attach is, effectively, a no-op.")]
        public void Attach_NoOp()
        {
            Assert.DoesNotThrow(() => new TenantIdentificationContextExtension().Attach(null));
        }

        [Test(Description = "Verifies that Detach is, effectively, a no-op.")]
        public void Detach_NoOp()
        {
            Assert.DoesNotThrow(() => new TenantIdentificationContextExtension().Detach(null));
        }
    }
}
