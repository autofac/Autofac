using System;
using AutofacContrib.Multitenant;

namespace MultitenantExample.WcfService
{
    public class StubTenantIdentificationStrategy : ITenantIdentificationStrategy
    {
        public bool TryIdentifyTenant(out object tenantId)
        {
            tenantId = null;
            return true;
        }
    }
}