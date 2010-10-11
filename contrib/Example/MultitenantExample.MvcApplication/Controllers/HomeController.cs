using System;
using System.Web.Mvc;
using AutofacContrib.Multitenant;
using MultitenantExample.MvcApplication.Dependencies;
using MultitenantExample.MvcApplication.Models;
using MultitenantExample.MvcApplication.WcfService;
using MultitenantExample.MvcApplication.WcfMetadataConsumer;

namespace MultitenantExample.MvcApplication.Controllers
{
	[HandleError]
	public class HomeController : Controller
	{
		public IDependency Dependency { get; set; }
		public ITenantIdentificationStrategy TenantIdentificationStrategy { get; set; }
		public IMultitenantService StandardServiceProxy { get; set; }
		public IMetadataConsumer MetadataServiceProxy { get; set; }

		public HomeController(IDependency dependency, ITenantIdentificationStrategy tenantIdStrategy, IMultitenantService standardService, IMetadataConsumer metadataService)
		{
			this.Dependency = dependency;
			this.TenantIdentificationStrategy = tenantIdStrategy;
			this.StandardServiceProxy = standardService;
			this.MetadataServiceProxy = metadataService;
		}

		public virtual ActionResult Index()
		{
			var model = this.BuildIndexModel();
			return View(model);
		}

		protected virtual IndexModel BuildIndexModel()
		{
			var model = new IndexModel()
			{
				ControllerTypeName = this.GetType().Name,
				DependencyInstanceId = this.Dependency.InstanceId,
				DependencyTypeName = this.Dependency.GetType().Name,
				TenantId = this.GetTenantId(),
				StandardServiceInfo = this.StandardServiceProxy.GetServiceInfo(new MultitenantExample.MvcApplication.WcfService.GetServiceInfoRequest()),
				MetadataServiceInfo = this.MetadataServiceProxy.GetServiceInfo(new MultitenantExample.MvcApplication.WcfMetadataConsumer.GetServiceInfoRequest())
			};
			return model;
		}

		private object GetTenantId()
		{
			object tenantId = null;
			bool success = this.TenantIdentificationStrategy.TryIdentifyTenant(out tenantId);
			if (!success || tenantId == null)
			{
				return "[Default Tenant]";
			}
			return tenantId;
		}

		public ActionResult About()
		{
			return View();
		}
	}
}
