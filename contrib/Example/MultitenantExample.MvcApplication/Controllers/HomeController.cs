using System;
using System.Web.Mvc;
using AutofacContrib.Multitenant;
using MultitenantExample.MvcApplication.Dependencies;
using MultitenantExample.MvcApplication.Models;
using MultitenantExample.MvcApplication.WcfService;

namespace MultitenantExample.MvcApplication.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public IDependency Dependency { get; set; }
        public ITenantIdentificationStrategy TenantIdentificationStrategy { get; set; }
        public IMultitenantService ServiceProxy { get; set; }

        public HomeController(IDependency dependency, ITenantIdentificationStrategy tenantIdStrategy, IMultitenantService serviceProxy)
        {
            this.Dependency = dependency;
            this.TenantIdentificationStrategy = tenantIdStrategy;
            this.ServiceProxy = serviceProxy;
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
                ServiceInfo = this.ServiceProxy.GetServiceInfo(new GetServiceInfoRequest())
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
