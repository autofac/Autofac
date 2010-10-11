using System;
using AutofacContrib.Multitenant;
using MultitenantExample.MvcApplication.Dependencies;
using MultitenantExample.MvcApplication.WcfService;
using MultitenantExample.MvcApplication.WcfMetadataConsumer;

namespace MultitenantExample.MvcApplication.Controllers
{
	/// <summary>
	/// Example of a tenant-specific controller for Tenant 2.
	/// </summary>
	/// <remarks>
	/// <para>
	/// You have to derive custom controllers from the original controller because
	/// you have to register them as the base controller type.
	/// </para>
	/// </remarks>
	public class Tenant2Controller : HomeController
	{
		public Tenant2Controller(IDependency dependency, ITenantIdentificationStrategy tenantIdStrategy, IMultitenantService standardService, IMetadataConsumer metadataService) :
			base(dependency, tenantIdStrategy, standardService, metadataService)
		{
		}

		protected override Models.IndexModel BuildIndexModel()
		{
			var model = base.BuildIndexModel();
			model.ControllerTypeName += " [Here is something custom done by the Tenant 2 controller.]";
			return model;
		}
	}
}