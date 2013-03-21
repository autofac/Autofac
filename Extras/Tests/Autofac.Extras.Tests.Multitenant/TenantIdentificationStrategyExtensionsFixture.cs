using System;
using Autofac;
using Autofac.Extras.Multitenant;
using Autofac.Extras.Tests.Multitenant.Stubs;
using NUnit.Framework;

namespace Autofac.Extras.Tests.Multitenant
{
    [TestFixture]
    public class TenantIdentificationStrategyExtensionsFixture
    {
        [Test(Description = "Retrieves the tenant ID but tries to convert to the wrong type.")]
        public void IdentifyTenant_FailedConversion()
        {
            var strategy = new StubTenantIdentificationStrategy
            {
                TenantId = Guid.NewGuid()
            };
            Assert.Throws<InvalidCastException>(() => strategy.IdentifyTenant<int>());
        }

        [Test(Description = "Attempts to retrieve the tenant ID when identification fails.")]
        public void IdentifyTenant_FailedRetrieval()
        {
            var strategy = new StubTenantIdentificationStrategy
            {
                IdentificationSuccess = false
            };
            Assert.AreEqual(Guid.Empty, strategy.IdentifyTenant<Guid>(), "The tenant ID should be the default for the type if identification fails.");
        }

        [Test(Description = "Attempts to get the tenant ID from a null strategy.")]
        public void IdentifyTenant_NullStrategy()
        {
            ITenantIdentificationStrategy strategy = null;
            Assert.Throws<ArgumentNullException>(() => strategy.IdentifyTenant<Guid>());
        }

        [Test(Description = "Successfully retrieves and converts the tenant ID.")]
        public void IdentifyTenant_SuccessfulRetrieval()
        {
            var expected = Guid.NewGuid();
            var strategy = new StubTenantIdentificationStrategy
            {
                TenantId = expected
            };
            Assert.AreEqual(expected, strategy.IdentifyTenant<Guid>(), "The tenant ID wasn't properly retrieved and parsed.");
        }
    }
}
