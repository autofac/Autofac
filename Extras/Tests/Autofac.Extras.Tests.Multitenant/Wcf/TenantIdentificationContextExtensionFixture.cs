using System;
using Autofac.Extras.Multitenant.Wcf;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant.Wcf
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
