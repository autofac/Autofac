using System.Web.Mvc;

namespace Remember.Web.Areas.DomainServices
{
    public class DomainServicesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DomainServices";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DomainServices_default",
                "DomainServices/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
