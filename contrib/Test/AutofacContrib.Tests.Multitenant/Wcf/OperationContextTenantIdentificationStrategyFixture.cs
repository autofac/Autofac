using System;
using AutofacContrib.Multitenant.Wcf;
using NUnit.Framework;

namespace AutofacContrib.Tests.Multitenant.Wcf
{
    [TestFixture]
    public class OperationContextTenantIdentificationStrategyFixture
    {
        [Test(Description = "If there is no operation context, the tenant should not be identified.")]
        public void TryIdentifyTenant_NoOperationContext()
        {
            var strategy = new OperationContextTenantIdentificationStrategy();
            object tenantId;
            bool success = strategy.TryIdentifyTenant(out tenantId);
            Assert.IsFalse(success, "The tenant should not be identified if there is no operation context.");
        }
    }
}
