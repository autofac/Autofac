using System;
using AutofacContrib.Multitenant.Web;
using NUnit.Framework;

namespace AutofacContrib.Tests.Multitenant.Web
{
    [TestFixture]
    public class RequestParameterTenantIdentificationStrategyFixture
    {
        [Test(Description = "Attempts to initialize the strategy with an empty parameter name.")]
        public void Ctor_EmptyParameterName()
        {
            Assert.Throws<ArgumentException>(() => new RequestParameterTenantIdentificationStrategy(""));
        }

        [Test(Description = "Attempts to initialize the strategy with a null parameter name.")]
        public void Ctor_NullParameterName()
        {
            Assert.Throws<ArgumentNullException>(() => new RequestParameterTenantIdentificationStrategy(null));
        }

        [Test(Description = "Verifies the constructor stores the passed-in parameter name.")]
        public void Ctor_StoresParameterName()
        {
            var strategy = new RequestParameterTenantIdentificationStrategy("foo");
            Assert.AreEqual("foo", strategy.ParameterName, "The parameter name was not stored.");
        }

        [Test(Description = "Attempts to resolve the tenant ID without a web context.")]
        public void TryIdentifyTenant_NoHttpContext()
        {
            var strategy = new RequestParameterTenantIdentificationStrategy("foo");
            object id;
            bool success = strategy.TryIdentifyTenant(out id);
            Assert.IsFalse(success, "Without a context, the tenant should not have been identified.");
        }
    }
}
