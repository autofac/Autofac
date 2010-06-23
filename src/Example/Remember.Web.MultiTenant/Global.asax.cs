using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using Autofac.Integration.Web.MultiTenant;
using Autofac.Integration.Web.Mvc;

namespace Remember.Web.Multitenant
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication, IContainerProviderAccessor
    {
        static MultiTenantContainerProvider _containerProvider;

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        // Extremely simple multi-tenancy .... if the query string contains 'tenantId=nick' then
        // a special greeting will be shown :)
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);

            var baseContainerBuilder = new ContainerBuilder();
            baseContainerBuilder.RegisterControllers(typeof(MvcApplication).Assembly);
            baseContainerBuilder.RegisterInstance("Hello").Named<string>("greeting");
            baseContainerBuilder.Register(c =>
                                          c.Resolve<string>("greeting") + ", " +
                                          c.Resolve<string>("audience"));

            var tenancyRegistry = new TenancyRegistry(baseContainerBuilder.Build());
            tenancyRegistry.ConfigureDefaultTenant(dt => dt.RegisterInstance("World").Named<string>("audience"));
            tenancyRegistry.ConfigureTenant("nick", n => n.RegisterInstance("Nicholas").Named<string>("audience"));

            _containerProvider = new MultiTenantContainerProvider(
                new RequestParameterTenantIdentificationStrategy("tenantId"),
                tenancyRegistry);

            ControllerBuilder.Current.SetControllerFactory(new AutofacControllerFactory(_containerProvider));
        }

        protected void Application_End()
        {
            _containerProvider.Dispose();
        }

        public IContainerProvider ContainerProvider
        {
            get { return _containerProvider; }
        }
    }
}