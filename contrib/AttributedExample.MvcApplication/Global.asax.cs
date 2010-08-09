using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AttributedExample.MvcApplication.Models;
using AttributedExample.MvcApplication.Models.Query;
using Autofac;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using AutofacContrib.Attributed;

namespace AttributedExample.MvcApplication
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication, IContainerProviderAccessor
    {
        /// <summary>
        /// Application container provider backing field. Part of standard Autofac web integration.
        /// </summary>
        private static IContainerProvider _containerProvider;

        /// <summary>
        /// Gets the global application container.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.Integration.Web.IContainerProvider"/> that
        /// returns the global application container.
        /// </value>
        /// <remarks>
        /// <para>
        /// This is part of standard Autofac web integration.
        /// </para>
        /// </remarks>
        public IContainerProvider ContainerProvider
        {
            get { return _containerProvider; }
        }

        /// <summary>
        /// Registers the application routes with a route collection.
        /// </summary>
        /// <param name="routes">
        /// The route collection with which to register routes.
        /// </param>
        /// <remarks>
        /// <para>
        /// This is part of standard MVC application setup.
        /// </para>
        /// </remarks>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        /// <summary>
        /// Handles the global application startup event.
        /// </summary>
        protected void Application_Start()
        {
            var builder = new ContainerBuilder();

            // this single call registers all IQueryModel implementations and grabs their metadata for later application
            builder.RegisterUsingMetadataAttributes<IQueryModel, IQueryModelMetadata>(Assembly.GetExecutingAssembly());

            builder.RegisterType<QueryPanelModel>().As<IQueryPanelModel>();
            builder.RegisterType<QueryHeaderModel>();
            builder.RegisterType<HomeModel>().As<IHomeModel>();
            builder.RegisterType<RunningState>();


            _containerProvider = new ContainerProvider(builder.Build());

            // Set the controller factory to use Autofac. This is standard
            // Autofac MVC integration.
            ControllerBuilder.Current.SetControllerFactory(new AutofacControllerFactory(this.ContainerProvider));

            // Perform the standard MVC setup requirements.
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);
        }
    }
}