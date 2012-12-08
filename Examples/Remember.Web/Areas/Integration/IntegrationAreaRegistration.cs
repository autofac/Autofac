using System.Web.Mvc;

namespace Remember.Web.Areas.Integration
{
    public class IntegrationAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Integration";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Integration_default",
                "Integration/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
