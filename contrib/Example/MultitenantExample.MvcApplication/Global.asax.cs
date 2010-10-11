using System;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using Autofac.Integration.Web.Mvc;
using AutofacContrib.Multitenant;
using AutofacContrib.Multitenant.Wcf;
using AutofacContrib.Multitenant.Web;
using MultitenantExample.MvcApplication.Controllers;
using MultitenantExample.MvcApplication.Dependencies;
using MultitenantExample.MvcApplication.WcfService;
using MultitenantExample.MvcApplication.WcfMetadataConsumer;

namespace MultitenantExample.MvcApplication
{
	/// <summary>
	/// Global application class for the multitenant MVC example application.
	/// </summary>
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
			// Create the tenant ID strategy. Required for multitenant integration.
			var tenantIdStrategy = new RequestParameterTenantIdentificationStrategy("tenant");

			// Register application-level dependencies and controllers. Note that
			// we are manually registering controllers rather than all at the same
			// time because some of the controllers in this sample application
			// are for specific tenants.
			var builder = new ContainerBuilder();
			builder.RegisterType<HomeController>();
			builder.RegisterType<BaseDependency>().As<IDependency>();

			// Adding the tenant ID strategy into the container so controllers
			// can display output about the current tenant.
			builder.RegisterInstance(tenantIdStrategy).As<ITenantIdentificationStrategy>();

			// The service client is not different per tenant because
			// the service itself is multitenant - one client for all
			// the tenants and the service implementation switches.
			builder.Register(c => new ChannelFactory<IMultitenantService>(new BasicHttpBinding(), new EndpointAddress("http://localhost:63578/MultitenantService.svc"))).SingleInstance();
			builder.Register(c => new ChannelFactory<IMetadataConsumer>(new WSHttpBinding(), new EndpointAddress("http://localhost:63578/MetadataConsumer.svc"))).SingleInstance();

			// Register an endpoint behavior on the client channel factory that
			// will propagate the tenant ID across the wire in a message header.
			// On the service side, you'll need to read the header from incoming
			// message headers to reconstitute the incoming tenant ID.
			builder.Register(c =>
			{
				var factory = c.Resolve<ChannelFactory<IMultitenantService>>();
				factory.Opening += (sender, args) => factory.Endpoint.Behaviors.Add(new TenantPropagationBehavior<string>(tenantIdStrategy));
				return factory.CreateChannel();
			}).InstancePerHttpRequest();
			builder.Register(c =>
			{
				var factory = c.Resolve<ChannelFactory<IMetadataConsumer>>();
				factory.Opening += (sender, args) => factory.Endpoint.Behaviors.Add(new TenantPropagationBehavior<string>(tenantIdStrategy));
				return factory.CreateChannel();
			}).InstancePerHttpRequest();

			// Create the multitenant container based on the application
			// defaults - here's where the multitenant bits truly come into play.
			var mtc = new MultitenantContainer(tenantIdStrategy, builder.Build());

			// Notice we configure tenant IDs as strings below because the tenant
			// identification strategy retrieves string values from the request
			// context. To use strongly-typed tenant IDs, create a custom tenant
			// identification strategy that returns the appropriate type.

			// Configure overrides for tenant 1 - dependencies, controllers, etc.
			mtc.ConfigureTenant("1",
				b =>
				{
					b.RegisterType<Tenant1Dependency>().As<IDependency>().InstancePerDependency();
					b.RegisterType<Tenant1Controller>().As<HomeController>();
				});

			// Configure overrides for tenant 2 - dependencies, controllers, etc.
			mtc.ConfigureTenant("2",
				b =>
				{
					b.RegisterType<Tenant2Dependency>().As<IDependency>().SingleInstance();
					b.RegisterType<Tenant2Controller>().As<HomeController>();
				});

			// Configure overrides for the default tenant. That means the default
			// tenant will have some different dependencies than other unconfigured
			// tenants.
			mtc.ConfigureTenant(null, b => b.RegisterType<DefaultTenantDependency>().As<IDependency>().SingleInstance());

			// Create the global application container provider using the
			// multitenant container instead of the application container.
			_containerProvider = new ContainerProvider(mtc);

			// Set the controller factory to use Autofac. This is standard
			// Autofac MVC integration.
			ControllerBuilder.Current.SetControllerFactory(new AutofacControllerFactory(this.ContainerProvider));

			// Perform the standard MVC setup requirements.
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);
		}
	}
}