using System;
using AutofacContrib.Multitenant;
using MultitenantExample.MvcApplication.Dependencies;
using MultitenantExample.MvcApplication.WcfService;

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
        public Tenant1Controller(IDependency dependency, ITenantIdentificationStrategy tenantIdStrategy, IMultitenantService serviceProxy) :
            base(dependency, tenantIdStrategy, serviceProxy)
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