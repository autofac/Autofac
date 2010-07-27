using System;
using AutofacContrib.Multitenant;

namespace AutofacContrib.Tests.Multitenant.Stubs
{
    public class StubTenantIdentificationStrategy : ITenantIdentificationStrategy
    {
        public bool IdentificationSuccess { get; set; }
        public object TenantId { get; set; }

        public StubTenantIdentificationStrategy()
        {
            this.IdentificationSuccess = true;
        }

        public bool TryIdentifyTenant(out object tenantId)
        {
            tenantId = this.TenantId;
            return this.IdentificationSuccess;
        }
    }
}
