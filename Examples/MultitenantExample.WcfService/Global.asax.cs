using System;
using Autofac;
using Autofac.Extras.Multitenant;
using Autofac.Extras.Multitenant.Wcf;
using Autofac.Integration.Wcf;
using MultitenantExample.WcfService.Dependencies;
using MultitenantExample.WcfService.ServiceImplementations;

namespace MultitenantExample.WcfService
{
    /// <summary>
    /// Global application class for the multitenant WCF example application.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is easiest to see this application in action from the MVC example.
    /// The MVC example makes use of this service and displays the information,
    /// illustrating a complete multitenant system in action.
    /// </para>
    /// </remarks>
    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// Handles the global application startup event.
        /// </summary>
        protected void Application_Start(object sender, EventArgs e)
        {
            // Create the tenant ID strategy. Required for multitenant integration.
            var tenantIdStrategy = new OperationContextTenantIdentificationStrategy();

            // Register application-level dependencies and service implementations.
            // Note that we are registering the services as the interface type
            // because the .svc files refer to the interfaces. We could potentially
            // use named service types as well.
            var builder = new ContainerBuilder();
            builder.RegisterType<BaseImplementation>().As<IMultitenantService>();
            builder.RegisterType<BaseImplementation>().As<IMetadataConsumer>();
            builder.RegisterType<BaseDependency>().As<IDependency>();

            // Adding the tenant ID strategy into the container so services
            // can return output about the current tenant.
            builder.RegisterInstance(tenantIdStrategy).As<ITenantIdentificationStrategy>();

            // Create the multitenant container based on the application
            // defaults - here's where the multitenant bits truly come into play.
            var mtc = new MultitenantContainer(tenantIdStrategy, builder.Build());

            // Notice we configure tenant IDs as strings below because the tenant
            // identification strategy retrieves string values from the message
            // headers.

            // Configure overrides for tenant 1 - dependencies, service implementations, etc.
            mtc.ConfigureTenant("1",
                b =>
                {
                    b.RegisterType<Tenant1Dependency>().As<IDependency>().InstancePerDependency();
                    b.RegisterType<Tenant1Implementation>().As<IMultitenantService>();
                    b.RegisterType<Tenant1Implementation>().As<IMetadataConsumer>();
                });

            // Configure overrides for tenant 2 - dependencies, service implementations, etc.
            mtc.ConfigureTenant("2",
                b =>
                {
                    b.RegisterType<Tenant2Dependency>().As<IDependency>().SingleInstance();
                    b.RegisterType<Tenant2Implementation>().As<IMultitenantService>();
                    b.RegisterType<Tenant2Implementation>().As<IMetadataConsumer>();
                });

            // Configure overrides for the default tenant. That means the default
            // tenant will have some different dependencies than other unconfigured
            // tenants.
            mtc.ConfigureTenant(null, b => b.RegisterType<DefaultTenantDependency>().As<IDependency>().SingleInstance());

            // Multitenant service hosting requires use of a different service implementation
            // data provider that will allow you to define a metadata buddy class that isn't
            // tenant-specific.
            AutofacHostFactory.ServiceImplementationDataProvider = new MultitenantServiceImplementationDataProvider();

            // Add a behavior to service hosts that get created so incoming messages
            // get inspected and the tenant ID can be parsed from message headers.
            // For multitenancy to work, you need to know for which tenant a
            // given request is being made. In this case, the incoming message headers
            // expect to see a string for the tenant ID; if your tenant ID coming
            // from clients is different, change that here.
            AutofacHostFactory.HostConfigurationAction =
                host =>
                    host.Opening += (s, args) =>
                        host.Description.Behaviors.Add(new TenantPropagationBehavior<string>(tenantIdStrategy));

            // Finally, set the host factory application container on the multitenant
            // WCF host to a multitenant container. This is similar to standard
            // Autofac WCF integration.
            AutofacHostFactory.Container = mtc;

            // Note that the .svc file for your service needs to use the
            // Autofac.Extras.Multitenant.Wcf.AutofacServiceHostFactory or
            // Autofac.Extras.Multitenant.Wcf.AutofacWebServiceHostFactory rather
            // than the standard Autofac host factories.
        }
    }
}