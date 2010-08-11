using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using Remember.Persistence.NHibernate;
using Autofac;
using Remember.Service;

namespace Remember.Web
{
    public class GlobalApplication : System.Web.HttpApplication, IContainerProviderAccessor
    {
        static IContainerProvider _containerProvider;

        

        static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            AreaRegistration.RegisterAllAreas();

            routes.MapRoute(
                "Default",                              // Route Name
                "{controller}/{action}/{id}",           // Route URL (pattern)
                new {                                   // Route Detauls
                    controller = "Home", 
                    action = "Index", 
                    id = UrlParameter.Optional 
                }, 
                new []{"Remember.Web.Controllers"}      // Route Namespaces that take preference
            );
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>();
            builder.RegisterModule(new NHibernateModule());

            _containerProvider = new ContainerProvider(builder.Build());

            ControllerBuilder.Current.SetControllerFactory(
                new AutofacControllerFactory(_containerProvider));

            RegisterRoutes(RouteTable.Routes);
        }

        public IContainerProvider ContainerProvider
        {
            get { return _containerProvider; }
        }
    }
}