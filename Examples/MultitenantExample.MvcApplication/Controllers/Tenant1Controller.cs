using System;
using Autofac.Extras.Multitenant;
using MultitenantExample.MvcApplication.Dependencies;
using MultitenantExample.MvcApplication.WcfService;
using MultitenantExample.MvcApplication.WcfMetadataConsumer;

namespace MultitenantExample.MvcApplication.Controllers
{
    /// <summary>
    /// Example of a tenant-specific controller for Tenant 1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You have to derive custom controllers from the original controller because
    /// you have to register them as the base controller type.
    /// </para>
    /// </remarks>
    public class Tenant1Controller : HomeController
    {
        public Tenant1Controller(IDependency dependency, ITenantIdentificationStrategy tenantIdStrategy, IMultitenantService standardService, IMetadataConsumer metadataService) :
            base(dependency, tenantIdStrategy, standardService, metadataService)
        {
        }

        protected override Models.IndexModel BuildIndexModel()
        {
            var model = base.BuildIndexModel();
            model.ControllerTypeName += " [This is custom text inserted by the Tenant 1 controller.]";
            return model;
        }
    }
}