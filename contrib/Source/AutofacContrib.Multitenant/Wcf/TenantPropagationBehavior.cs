using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Autofac;

namespace AutofacContrib.Multitenant.Wcf
{
    /// <summary>
    /// Behavior for WCF clients and service hosts that is used to propagate
    /// tenant ID from client to service.
    /// </summary>
    /// <typeparam name="TTenantId">
    /// The type of the tenant ID to propagate. Must be nullable and
    /// serializable so it can be added to a message header.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// This behavior applies the <see cref="AutofacContrib.Multitenant.Wcf.TenantPropagationMessageInspector{TTenantId}"/>
    /// to WCF clients and service hosts to automatically get the tenant ID on
    /// the WCF client end, add the ID to a header on the outbound message, and
    /// have the tenant ID read from headers on the service side and added to the
    /// operation context in an
    /// <see cref="AutofacContrib.Multitenant.Wcf.TenantIdentificationContextExtension"/>.
    /// This allows you, on the service side, to use the
    /// <see cref="AutofacContrib.Multitenant.Wcf.OperationContextTenantIdentificationStrategy"/>
    /// as your registered <see cref="AutofacContrib.Multitenant.ITenantIdentificationStrategy"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// In the following examples, the tenant ID is a <see cref="System.String"/>,
    /// so the <typeparamref name="TTenantId"/> in the examples corresponds.
    /// In your application, your tenant ID may be a nullable GUID or some other
    /// object, so you'd need to update accordingly. That would mean passing
    /// a different type as <typeparamref name="TTenantId"/> and implementing
    /// a custom <see cref="AutofacContrib.Multitenant.ITenantIdentificationStrategy"/>
    /// that parses the appropriate tenant ID from the execution context.
    /// </para>
    /// <para>
    /// The following snippet shows what registration of this behavior
    /// might look like in an ASP.NET application that consumes WCF services.
    /// </para>
    /// <code lang="C#">
    /// public class MvcApplication : HttpApplication, IContainerProviderAccessor
    /// {
    ///   private static IContainerProvider _containerProvider;
    /// 
    ///   public IContainerProvider ContainerProvider
    ///   {
    ///     get { return _containerProvider; }
    ///   }
    /// 
    ///   public static void RegisterRoutes(RouteCollection routes)
    ///   {
    ///     // Register your routes as normal.
    ///   }
    /// 
    ///   protected void Application_Start()
    ///   {
    ///     // Create a tenant ID strategy that will get the tenant from
    ///     // your ASP.NET request context.
    ///     var tenantIdStrategy = new RequestParameterTenantIdentificationStrategy("tenant");
    /// 
    ///     // Register application-level dependencies and controllers.
    ///     var builder = new ContainerBuilder();
    ///     builder.RegisterType&lt;HomeController&gt;();
    ///     // ... and so on.
    /// 
    ///     // When you register the WCF service client, add the
    ///     // TenantPropagationBehavior to the Opening event:
    /// 
    ///     builder.Register(
    ///       c =&gt; new ChannelFactory&lt;IMultitenantService&gt;(
    ///         new BasicHttpBinding(),
    ///         new EndpointAddress("http://server/TheService.svc"))).SingleInstance();
    ///     builder.Register(
    ///       c =&gt;
    ///       {
    ///         var factory = c.Resolve&lt;ChannelFactory&lt;IMultitenantService&gt;&gt;();
    ///         factory.Opening +=
    ///           (sender, args) =&gt;
    ///             factory.Endpoint.Behaviors.Add(
    ///             new TenantPropagationBehavior&lt;string&gt;(tenantIdStrategy);
    ///         return factory.CreateChannel()
    ///       }).InstancePerHttpRequest();
    /// 
    ///     // Create the multitenant container.
    ///     var mtc = new MultitenantContainer(tenantIdStrategy, builder.Build());
    /// 
    ///     // Register tenant specific overrides and set up the
    ///     // application container provider.
    ///     _containerProvider = new ContainerProvider(mtc);
    /// 
    ///     // Do other MVC setup like route registration, etc.
    ///     ControllerBuilder.Current.SetControllerFactory(new AutofacControllerFactory(this.ContainerProvider));
    ///     AreaRegistration.RegisterAllAreas();
    ///     RegisterRoutes(RouteTable.Routes);
    ///   }
    /// }
    /// </code>
    /// <para>
    /// Note that much of the above code is the standard ASP.NET application
    /// wireup with Autofac. The important part is when you register the service
    /// client - it needs to have a <see cref="AutofacContrib.Multitenant.Wcf.TenantPropagationBehavior{TTenantId}"/>
    /// attached to it that can get the container provider from the
    /// current application.
    /// </para>
    /// <para>
    /// The following snippet shows what registration of this behavior
    /// looks like in a WCF application hosted in IIS:
    /// </para>
    /// <code lang="C#">
    /// public class Global : System.Web.HttpApplication
    /// {
    ///   protected void Application_Start(object sender, EventArgs e)
    ///   {
    ///       // Create the tenant ID strategy. Required for multitenant integration.
    ///       var tenantIdStrategy = new OperationContextTenantIdentificationStrategy();
    /// 
    ///       // Register application-level dependencies and service implementations.
    ///       var builder = new ContainerBuilder();
    ///       builder.RegisterType&lt;BaseImplementation&gt;().As&lt;IMultitenantService&gt;();
    ///       // ... and so on.
    /// 
    ///       // Create the multitenant container.
    ///       var mtc = new MultitenantContainer(tenantIdStrategy, builder.Build());
    /// 
    ///       // Configure tenant-specific overrides and set the WCF host container.
    ///       AutofacContrib.Multitenant.Wcf.AutofacHostFactory.Container = mtc;
    /// 
    ///       // Add a behavior to service hosts that get created so incoming messages
    ///       // get inspected and the tenant ID can be parsed from message headers.
    ///       AutofacContrib.Multitenant.Wcf.AutofacHostFactory.HostConfigurationAction =
    ///         host =&gt;
    ///           host.Opening += (s, args) =&gt;
    ///             host.Description.Behaviors.Add(new TenantPropagationBehavior&lt;string&gt;(tenantIdStrategy));
    ///   }
    /// }
    /// </code>
    /// <para>
    /// Note that it is also very similar to standard wireup with Autofac WCF
    /// integration, just that you use the multitenant WCF host, a multitenant
    /// container, and a behavior to get the tenant ID from the operation context.
    /// </para>
    /// </example>
    /// <seealso cref="AutofacContrib.Multitenant.Wcf.TenantPropagationMessageInspector{TTenantId}"/>
    /// <seealso cref="AutofacContrib.Multitenant.Wcf.OperationContextTenantIdentificationStrategy"/>
    public class TenantPropagationBehavior<TTenantId> : IServiceBehavior, IEndpointBehavior
    {
        /// <summary>
        /// Gets the strategy used for identifying the current tenant.
        /// </summary>
        /// <value>
        /// An <see cref="AutofacContrib.Multitenant.ITenantIdentificationStrategy"/>
        /// used to identify the current tenant from the execution context.
        /// </value>
        public ITenantIdentificationStrategy TenantIdentificationStrategy { get; private set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="AutofacContrib.Multitenant.Wcf.TenantPropagationBehavior{TTenantId}"/> class.
        /// </summary>
        /// <param name="tenantIdentificationStrategy">
        /// The strategy to use for identifying the current tenant.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tenantIdentificationStrategy" /> is <see langword="null" />.
        /// </exception>
        public TenantPropagationBehavior(ITenantIdentificationStrategy tenantIdentificationStrategy)
        {
            if (tenantIdentificationStrategy == null)
            {
                throw new ArgumentNullException("tenantIdentificationStrategy");
            }
            this.TenantIdentificationStrategy = tenantIdentificationStrategy;
        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">
        /// The objects that binding elements require to support the behavior.
        /// </param>
        public virtual void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">
        /// Custom objects to which binding elements have access.
        /// </param>
        public virtual void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Adds the <see cref="AutofacContrib.Multitenant.Wcf.TenantPropagationMessageInspector{TTenantId}"/>
        /// to the client.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="clientRuntime" /> is <see langword="null" />.
        /// </exception>
        public virtual void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if (clientRuntime == null)
            {
                throw new ArgumentNullException("clientRuntime");
            }
            clientRuntime.MessageInspectors.Add(new TenantPropagationMessageInspector<TTenantId>(this.TenantIdentificationStrategy));
        }

        /// <summary>
        /// Adds the <see cref="AutofacContrib.Multitenant.Wcf.TenantPropagationMessageInspector{TTenantId}"/>
        /// to the service endpoints.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="serviceHostBase" /> is <see langword="null" />.
        /// </exception>
        public virtual void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase == null)
            {
                throw new ArgumentNullException("serviceHostBase");
            }
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                {
                    endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new TenantPropagationMessageInspector<TTenantId>(this.TenantIdentificationStrategy));
                }
            }
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public virtual void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public virtual void Validate(ServiceEndpoint endpoint)
        {
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public virtual void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
