========================
Multitenant Applications
========================

``Autofac.Extras.Multitenant`` enables multitenant dependency injection support.

.. contents::
  :local:

What Is Multitenancy?
=====================

A **multitenant application** is an application that you can deploy one time yet allow separate customers, or "tenants," to view the application as though it was their own.

Consider, for example, a hosted online store application - you, *the tenant*, lease the application, set some configuration values, and when an end user visits the application under your custom domain name, it looks like your company. Other tenants may also lease the application, yet the application is deployed only one time on a central, hosted server and changes its behavior based on the tenant (or tenant's end-users) accessing it.

Many changes in a multitenant environment are performed via simple configuration. For example, the colors or fonts displayed in the UI are simple configuration options that can be "plugged in" without actually changing the behavior of the application.

In a more complex scenario, **you may need to change business logic on a per-tenant basis.** For example, a specific tenant leasing space on the application may want to change the way a value is calculated using some complex custom logic. **How do you register a default behavior/dependency for an application and allow a specific tenant to override it?**

This is the functionality that ``Autofac.Extras.Multitenant`` attempts to address.

General Principles
==================

In general, a multitenant application has four tasks that need to be performed with respect to dependency resolution:

#. :ref:`reference_packages`
#. :ref:`register_dependencies`
#. :ref:`tenant_identification`
#. :ref:`resolve_dependencies`

This section outlines how these three steps work. Later sections will expand on these topics to include information on how to integrate these principles with specific application types.

.. _reference_packages:

Reference NuGet Packages
------------------------

Any application that wants to use multitenancy needs to add references to the NuGet packages...

- Autofac
- Autofac.Extras.Multitenant

That's the bare minimum. **WCF applications** also need ``Autofac.Extras.Multitenant.Wcf``.

.. _register_dependencies:

Register Dependencies
---------------------

``Autofac.Extras.Multitenant`` introduces a new container type called ``Autofac.Extras.Multitenant.MultitenantContainer``. This container is used for managing application-level defaults and tenant-specific overrides.

The overall registration process is:

#. **Create an application-level default container.** This container is where you register the default dependencies for the application. If a tenant doesn't otherwise provide an override for a dependency type, the dependencies registered here will be used.
#. **Instantiate a tenant identification strategy.** A tenant identification strategy is used to determine the ID for the current tenant based on execution context. You can read more on this later in this document.
#. **Create a multitenant container.** The multitenant container is responsible for keeping track of the application defaults and the tenant-specific overrides.
#. **Register tenant-specific overrides.** For each tenant wishing to override a dependency, register the appropriate overrides passing in the tenant ID and a configuration lambda.

General usage looks like this:

.. sourcecode:: csharp

    // First, create your application-level defaults using a standard
    // ContainerBuilder, just as you are used to.
    var builder = new ContainerBuilder();
    builder.RegisterType<Consumer>().As<IDependencyConsumer>().InstancePerDependency();
    builder.RegisterType<BaseDependency>().As<IDependency>().SingleInstance();
    var appContainer = builder.Build();

    // Once you've built the application-level default container, you
    // need to create a tenant identification strategy. The details of this
    // are discussed later on.
    var tenantIdentifier = new MyTenantIdentificationStrategy();

    // Now create the multitenant container using the application
    // container and the tenant identification strategy.
    var mtc = new MultitenantContainer(tenantIdentifier, appContainer);

    // Configure the overrides for each tenant by passing in the tenant ID
    // and a lambda that takes a ContainerBuilder.
    mtc.ConfigureTenant('1', b => b.RegisterType<Tenant1Dependency>().As<IDependency>().InstancePerDependency());
    mtc.ConfigureTenant('2', b => b.RegisterType<Tenant2Dependency>().As<IDependency>().SingleInstance());

    // Now you can use the multitenant container to resolve instances.

**If you have a component that needs one instance per tenant**, you can use the ``InstancePerTenant()`` registration extension method at the container level.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<SomeType>().As<ISomeInterface>().InstancePerTenant();
    // InstancePerTenant goes in the main container; other
    // tenant-specific dependencies get registered as shown
    // above, in tenant-specific lifetimes.

Note that **you may only configure a tenant one time.** After that, you may not change that tenant's overrides. Also, if you resolve a dependency for a tenant, their lifetime scope may not be changed. It is good practice to configure your tenant overrides at application startup to avoid any issues. If you need to perform some business logic to "build" the tenant configuration, you can use the ``Autofac.Extras.Multitenant.ConfigurationActionBuilder``.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    // ... register things...
    var appContainer = builder.Build();
    var tenantIdentifier = new MyTenantIdentificationStrategy();
    var mtc = new MultitenantContainer(tenantIdentifier, appContainer);

    // Create a configuration action builder to aggregate registration
    // actions over the course of some business logic.
    var actionBuilder = new ConfigurationActionBuilder();

    // Do some logic...
    if(SomethingIsTrue())
    {
      actionBuilder.Add(b => b.RegisterType<AnOverride>().As<ISomething>());
    }
    actionBuilder.Add(b => b.RegisterType<SomeClass>());
    if(AnotherThingIsTrue())
    {
      actionBuilder.Add(b => b.RegisterModule<MyModule>());
    }

    // Now configure a tenant using the built action.
    mtc.ConfigureTenant('1', actionBuilder.Build());

.. _tenant_identification:

Identify the Tenant
-------------------

In order to resolve a tenant-specific dependency, Autofac needs to know which tenant is making the resolution request. That is, "for the current execution context, which tenant is resolving dependencies?"

Autofac.Extras.Multitenant includes an ``ITenantIdentificationStrategy`` interface that you can implement to provide just such a mechanism. This allows you to retrieve the tenant ID from anywhere appropriate to your application: an environment variable, a role on the current user's principal, an incoming request value, or anywhere else.

The following example shows what a simple ``ITenantIdentificationStrategy`` that a web application might look like.

.. sourcecode:: csharp

    using System;
    using System.Web;
    using Autofac.Extras.Multitenant;

    namespace DemoNamespace
    {
      public class RequestParameterStrategy : ITenantIdentificationStrategy
      {
        public bool TryIdentifyTenant(out object tenantId)
        {
          // This is an EXAMPLE ONLY and is NOT RECOMMENDED.
          tenantId = null;
          try
          {
            var context = HttpContext.Current;
            if(context != null && context.Request != null)
            {
              tenantId = context.Request.Params["tenant"];
            }
          }
          catch(HttpException)
          {
            // Happens at app startup in IIS 7.0
          }
          return tenantId != null;
        }
      }
    }

In this example, a web application is using an incoming request parameter to get the tenant ID. (Note that **this is just an example and is not recommended** because it would allow any user on the system to very easily just switch tenants.) A slightly more robust version of this exact strategy is provided as ``Autofac.Extras.Multitenant.Web.RequestParameterTenantIdentificationStrategy`` but, again, is still not recommended for production due to the insecurity.

In your custom strategy implementation, you may choose to represent your tenant IDs as GUIDs, integers, or any other custom type. The strategy here is where you would parse the value from the execution context into a strongly typed object and succeed/fail based on whether the value is present and/or whether it can be parsed into the appropriate type.

``Autofac.Extras.Multitenant`` uses ``System.Object`` as the tenant ID type throughout the system for maximum flexibility.

**Performance is important in tenant identification.** Tenant identification happens every time you resolve a component, begin a new lifetime scope, etc. As such, it is very important to make sure your tenant identification strategy is fast. For example, you wouldn't want to do a service call or a database query during tenant identification.

**Be sure to handle errors well in tenant identification.** Especially in situations like ASP.NET application startup, you may use some contextual mechanism (like ``HttpContext.Current.Request``) to determine your tenant ID, but if your tenant ID strategy gets called when that contextual information isn't available, you need to be able to handle that. You'll see in the above example that not only does it check for the current ``HttpContext``, but also the ``Request``. Check everything and handle exceptions (e.g., parsing exceptions) or you may get some odd or hard-to-troubleshoot behavior.

.. _resolve_dependencies:

Resolve Tenant-Specific Dependencies
------------------------------------

The way the ``MultitenantContainer`` works, each tenant on the system gets their own ``Autofac.ILifetimeScope`` instance which contains the set of application defaults along with the tenant-specific overrides. Doing this...

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<BaseDependency>().As<IDependency>().SingleInstance();
    var appContainer = builder.Build();

    var tenantIdentifier = new MyTenantIdentificationStrategy();

    var mtc = new MultitenantContainer(tenantIdentifier, appContainer);
    mtc.ConfigureTenant('1', b => b.RegisterType<Tenant1Dependency>().As<IDependency>().InstancePerDependency());

Is very much like using the standard ``ILifetimeScope.BeginLifetimeScope(Action<ContainerBuilder>)``, like this:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<BaseDependency>().As<IDependency>().SingleInstance();
    var appContainer = builder.Build();

    using(var scope = appContainer.BeginLifetimeScope(
      b => b.RegisterType<Tenant1Dependency>().As<IDependency>().InstancePerDependency())
      {
        // Do work with the created scope...
      }

When you use the ``MultitenantContainer`` to resolve a dependency, behind the scenes it calls your ``ITenantIdentificationStrategy`` to identify the tenant, it locates the tenant's lifetime scope (with their configured overrides), and resolves the dependency from that scope. It does all this transparently, so you can use the multitenant container the same as you do other containers.

.. sourcecode:: csharp

    var dependency = mtc.Resolve<IDependency>();
    // "dependency" will be a tenant-specific value resolved from
    // the multitenant container. If the current tenant has overridden
    // the IDependency registration, that override will be resolved;
    // otherwise it will be the application-level default.


The important bit here is that all the work is going on transparently behind the scenes. Any call to ``Resolve``, ``BeginLifetimeScope``, ``Tag``, ``Disposer``, or the other methods/properties on the ``IContainer`` interface will all go through the tenant identification process and the result of the call will be tenant-specific.

If you need to specifically access a tenant's lifetime scope or the application container, the ``MultitenantContainer`` provides:

- ``ApplicationContainer``: Gets the application container.
- ``GetCurrentTenantScope``: Identifies the current tenant and returns their specific lifetime scope.
- ``GetTenantScope``: Allows you to provide a specific tenant ID for which you want the lifetime scope.

.. _aspnet_integration:

ASP.NET Integration
===================

ASP.NET integration is not really any different than :doc:`standard ASP.NET application integration <../integration/aspnet>`. Really, the only difference is that you will set up your application's ``Autofac.Integration.Web.IContainerProvider`` or ``System.,Web.Mvc.IDependencyResolver`` or whatever with an ``Autofac.Extras.Multitenant.MultitenantContainer`` rather than a regular container built by a ``ContainerBuilder``. Since the ``MultitenantContainer`` handles multitenancy in a transparent fashion, "things just work."

ASP.NET Application Startup
---------------------------

Here is a sample :doc:`ASP.NET MVC <../integration/mvc>` ``Global.asax`` implementation illustrating how simple it is:

.. sourcecode:: csharp

    namespace MultitenantExample.MvcApplication
    {
      public class MvcApplication : HttpApplication
      {
        public static void RegisterRoutes(RouteCollection routes)
        {
          // Register your routes - standard MVC stuff.
        }

        protected void Application_Start()
        {
          // Set up the tenant ID strategy and application container.
          // The request parameter tenant ID strategy is used here as an example.
          // You should use your own strategy in production.
          var tenantIdStrategy = new RequestParameterTenantIdentificationStrategy("tenant");
          var builder = new ContainerBuilder();
          builder.RegisterType<BaseDependency>().As<IDependency>();

          // If you have tenant-specific controllers in the same assembly as the
          // application, you should register controllers individually.
          builder.RegisterType<HomeController>();

          // Create the multitenant container and the tenant overrides.
          var mtc = new MultitenantContainer(tenantIdStrategy, builder.Build());
          mtc.ConfigureTenant("1",
            b =>
            {
              b.RegisterType<Tenant1Dependency>().As<IDependency>().InstancePerDependency();
              b.RegisterType<Tenant1Controller>().As<HomeController>();
            });

          // Here's the magic line: Set up the DependencyResolver using
          // a multitenant container rather than a regular container.
          DependencyResolver.SetResolver(new AutofacDependencyResolver(mtc));

          // ...and everything else is standard MVC.
          AreaRegistration.RegisterAllAreas();
          RegisterRoutes(RouteTable.Routes);
        }
      }
    }

As you can see, **it's almost the same as regular MVC Autofac integration**. You set up the application container, the tenant ID strategy, the multitenant container, and the tenant overrides as illustrated earlier in :ref:`register_dependencies` and :ref:`tenant_identification`. Then when you set up your ``DependencyResolver``, give it the multitenant container. Everything else just works.

**This similarity is true for other web applications** as well. When setting up your ``IContainerProviderAccessor`` for web forms, use the multitenant container instead of the standard container. When setting up your :doc:`Web API <../integration/webapi>` ``DependencyResolver``, use the multitenant container instead of the standard container.

Note in the example that controllers are getting registered individually rather than using the all-at-once ``builder.RegisterControllers(Assembly.GetExecutingAssembly());`` style of registration. See below for more on why this is the case.

Tenant-Specific Controllers
---------------------------

You may choose, in an MVC application, to allow a tenant to override a controller. This is possible, but requires a little forethought.

First, **tenant-specific controllers must derive from the controller they are overriding.** For example, if you have a ``HomeController`` for your application and a tenant wants to create their own implementation of it, they need to derive from it, like...

.. sourcecode:: csharp

    public class Tenant1HomeController : HomeController
    {
      // Tenant-specific implementation of the controller.
    }

Second, **if your tenant-specific controllers are in the same assembly as the rest of the application, you can't register your controllers in one line.** You may have seen in standard :doc:`ASP.NET MVC integration <../integration/mvc>` a line like ``builder.RegisterControllers(Assembly.GetExecutingAssembly());`` to register all the controllers in the assembly at once. Unfortunately, if you have tenant-specific controllers in the same assembly, they'll all be registered at the application level if you do this. Instead, you need to register each application controller at the application level one at a time, and then configure tenant-specific overrides the same way.

The example ``Global.asax`` above shows this pattern of registering controllers individually.

Of course, if you keep your tenant-specific controllers in other assemblies, you can register all of the application controllers at once using ``builder.RegisterControllers(Assembly.GetExecutingAssembly());`` and it'll work just fine. Note that if your tenant-specific controller assemblies aren't referenced by the main application (e.g., they're "plugins" that get dynamically registered at startup using assembly probing or some such) :doc:`you'll need to register your assemblies with the ASP.NET BuildManager <../integration/mvc>`.

Finally, when registering tenant-specific controllers, register them "as" the base controller type. In the example above, you see the default controller registered in the application container like this:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<HomeController>();

Then when the tenant overrides the controller in their tenant configuration, it looks like this:

.. sourcecode:: csharp

    var mtc = new MultitenantContainer(tenantIdStrategy, builder.Build());
    mtc.ConfigureTenant("1", b => b.RegisterType<Tenant1Controller>().As<HomeController>());


**Due to the relative complexity of this, it may be a better idea to isolate business logic into external dependencies that get passed into your controllers so the tenants can provide override dependencies rather than override controllers.**

.. _wcf_integration:

WCF Integration
===============

WCF integration is just slightly different than the :doc:`standard WCF integration <../integration/wcf>` in that you need to use a different service host factory than the standard Autofac host factory and there's a little additional configuration required.

Also, identifying a tenant is a little harder - the client needs to pass the tenant ID to the service somehow and the service needs to know how to interpret that passed tenant ID. A simple solution to this is provided in the form of a behavior that passes the relevant information in message headers.

Reference Packages for WCF Integration
--------------------------------------

For an application **consuming a multitenant service** (a client application), add references to...

- Autofac
- Autofac.Extras.Multitenant

For an application **providing a multitenant service** (a service application), add references to...

- Autofac
- Autofac.Integration.Wcf
- Autofac.Extras.Multitenant
- Autofac.Extras.Multitenant.Wcf

.. _behavior_id:

Passing Tenant ID with a Behavior
---------------------------------

As mentioned earlier (:ref:`tenant_identification`), for multitenancy to work you have to identify which tenant is making a given call so you can resolve the appropriate dependencies. One of the challenges in a service environment is that the tenant is generally established on the client application end and that tenant ID needs to be propagated to the service so it can behave appropriately.

A common solution to this is to propagate the tenant ID in message headers. The client adds a special header to an outgoing message that contains the tenant ID. The service parses that header, reads out the tenant ID, and uses that ID to determine its functionality.

In WCF, the way to attach these "dynamic" headers to messages and read them back is through a behavior. You apply the behavior to both the client and the service ends so the same header information (type, URN, etc.) is used.

``Autofac.Extras.Multitenant`` provides a simple tenant ID propagation behavior in ``Autofac.Extras.Multitenant.Wcf.TenantPropagationBehavior``. Applied on the client side, it uses the tenant ID strategy to retrieve the contextual tenant ID and insert it into a message header on an outgoing message. Applied on the server side, it looks for this inbound header and parses the tenant ID out, putting it into an OperationContext extension.

The :ref:`wcf_startup` section below shows examples of putting this behavior in action both on the client and server sides.

If you use this behavior, a corresponding server-side tenant identification strategy is also provided for you. See :ref:`operationcontext_id`, below.

.. _operationcontext_id:

Tenant Identification from OperationContext
-------------------------------------------

Whether or not you choose to use the provided ``Autofac.Extras.Multitenant.Wcf.TenantPropagationBehavior`` to propagate behavior from client to server in a message header (see above :ref:`behavior_id`), a good place to store the tenant ID for the life of an operation is in the ``OperationContext``.

``Autofac.Extras.Multitenant`` provides the ``Autofac.Extras.Multitenant.Wcf.TenantIdentificationContextExtension`` as an extension to the WCF ``OperationContext`` for just this purpose.

Early in the operation lifecycle (generally in a `System.ServiceModel.Dispatcher.IDispatchMessageInspector.AfterReceiveRequest() <http://msdn.microsoft.com/en-us/library/system.servicemodel.dispatcher.idispatchmessageinspector.afterreceiverequest.aspx>`_ implementation), you can add the ``TenantIdentificationContextExtension`` to the current ``OperationContext`` so the tenant can be easily identified. A sample ``AfterReceiveRequest()`` implementation below shows this in action:

.. sourcecode:: csharp

    public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
    {
      // This assumes the tenant ID is coming from a message header; you can
      // get it from wherever you want.
      var tenantId = request.Headers.GetHeader<TTenantId>(TenantHeaderName, TenantHeaderNamespace);

      // Here's where you add the context extension:
      OperationContext.Current.Extensions.Add(new TenantIdentificationContextExtension() { TenantId = tenantId });
      return null;
    }

Once the tenant ID is attached to the context, you can use an appropriate ``ITenantIdentificationStrategy`` to retrieve it as needed.

**If you use the TenantIdentificationContextExtension, then the provided Autofac.Extras.Multitenant.Wcf.OperationContextTenantIdentificationStrategy will automatically work to get the tenant ID from OperationContext.**

.. _hosting:

Hosting Multitenant Services
----------------------------

In a WCF service application, service implementations may be tenant-specific yet share the same service contract. This allows you to provide your service contracts in a separate assembly to tenant-specific developers and allow them to implement custom logic without sharing any of the internals of your default implementation.

To enable this to happen, a custom strategy has been implemented for multitenant service location - ``Autofac.Extras.Multitenant.Wcf.MultitenantServiceImplementationDataProvider``.

In your service's ``.svc`` file, you must specify:

- **The full type name of the service contract interface.** In regular :doc:`WCF integration <../integration/wcf>` Autofac allows you to use either typed or named services. For multitenancy, you must use a typed service that is based on the service contract interface.
- **The full type name of the Autofac host factory.** This lets the hosting environment know which factory to use. (This is just like the :doc:`standard Autofac WCF integration <../integration/wcf>`.)

An example ``.svc`` file looks like this:

.. sourcecode:: aspx-cs

    <%@ ServiceHost
        Service="MultitenantExample.WcfService.IMultitenantService, MultitenantExample.WcfService"
        Factory="Autofac.Integration.Wcf.AutofacServiceHostFactory, Autofac.Integration.Wcf" %>

When registering service implementations with the Autofac container, you must register the implementations as the contract interface, like this:

.. sourcecode:: csharp

    builder.RegisterType<BaseImplementation>().As<IMultitenantService>();

Tenant-specific overrides may then register using the interface type as well:

.. sourcecode:: csharp

    mtc.ConfigureTenant("1", b =>b.RegisterType<Tenant1Implementation>().As<IMultitenantService>());

And don't forget at app startup, around where you set the container, you need to tell Autofac you're doing multitenancy:

.. sourcecode:: csharp

    AutofacHostFactory.ServiceImplementationDataProvider =
      new MultitenantServiceImplementationDataProvider();

Managing Service Attributes
"""""""""""""""""""""""""""

When configuring WCF services in XML configuration (e.g., web.config), WCF automatically infers the name of the service element it expects from the concrete service implementation type. For example, in a single-tenant implementation, your ``MyNamespace.IMyService`` service interface might have one implementation called ``MyNamespace.MyService`` and that's what WCF would expect to look for in ``web.config``, like this:

.. sourcecode:: xml

    <system.serviceModel>
      <services>
        <service name="MyNamespace.MyService" ... />
      </services>
    </system.serviceModel>

However, when using a multitenant service host, the concrete service type that implements the interface is a dynamically generated proxy type, so the service configuration name becomes an auto-generated type name, like this:

.. sourcecode:: xml

    <system.serviceModel>
      <services>
        <service name="Castle.Proxies.IMyService_1" ... />
      </services>
    </system.serviceModel>

To make this easier, ``Autofac.Extras.Multitenant`` provides the ``Autofac.Extras.Multitenant.Wcf.ServiceMetadataTypeAttribute``, which you can use to create a "metadata buddy class" (similar to the ``System.ComponentModel.DataAnnotations.MetadataTypeAttribute``) that you can mark with type-level attributes and modify the behavior of the dynamic proxy.

In this case, you need the dynamic proxy to have a ``System.ServiceModel.ServiceBehaviorAttribute`` so you can define the ``ConfigurationName`` to expect.

First, mark your service interface with a ``ServiceMetadataTypeAttribute``:

.. sourcecode:: csharp

    using System;
    using System.ServiceModel;
    using Autofac.Extras.Multitenant.Wcf;

    namespace MyNamespace
    {
      [ServiceContract]
      [ServiceMetadataType(typeof(MyServiceBuddyClass))]
      public interface IMyService
      {
        // ...define your service operations...
      }
    }

Next, create the buddy class you specified in the attribute and add the appropriate metadata.

.. sourcecode:: csharp

    using System;
    using System.ServiceModel;

    namespace MyNamespace
    {
      [ServiceBehavior(ConfigurationName = "MyNamespace.IMyService")]
      public class MyServiceBuddyClass
      {
      }
    }

Now in your XML configuration file, you can use the configuration name you specified on the buddy class:

.. sourcecode:: xml

    <system.serviceModel>
      <services>
        <service name="MyNamespace.IMyService" ... />
      </services>
    </system.serviceModel>

**Important notes about metadata**:
- **Only type-level attributes are copied.** At this time, only attributes at the type level are copied over from the buddy class to the dynamic proxy. If you have a use case for property/method level metadata to be copied, please file an issue.
- **Not all metadata will have the effect you expect.** For example, if you use the ``ServiceBehaviorAttribute`` to define lifetime related information like ``InstanceContextMode``, the service will not follow that directive because Autofac is managing the lifetime, not the standard service host. Use common sense when specifying metadata - if it doesn't work, don't forget you're not using the standard service lifetime management functionality.
- **Metadata is application-level, not per-tenant.** The metadata buddy class info will take effect at an application level and can't be overridden per tenant.


Tenant-Specific Service Implementations
"""""""""""""""""""""""""""""""""""""""

If you are hosting multitenant services (:ref:`hosting`), you can provide tenant-specific service implementations. This allows you to provide a base implementation of a service and share the service contract with tenants to allow them to develop custom service implementations.

**You must implement your service contract as a separate interface**. You can't mark your service implementation with the ``ServiceContractAttribute``. Your service implementations must then implement the interface. This is good practice anyway, but the multitenant service host won't allow concrete types to directly define the contract.

Tenant-specific service implementations do not need to derive from the base implementation; they only need to implement the service interface.

You can register tenant-specific service implementations in app startup (see :ref:`wcf_startup`).

.. _wcf_startup:

WCF Application Startup
-----------------------

Application startup is generally the same as any other multitenant application (:ref:`register_dependencies`), but there are a couple of minor things to do for clients, and a little bit of hosting setup for services.

WCF Client Application Startup
""""""""""""""""""""""""""""""

**In a WCF client application**, when you register your service clients you'll need to register the behavior that propagates the tenant ID to the service. If you're following the :doc:`standard WCF integration guidance <../integration/wcf>`, then registering a service client looks like this:

.. sourcecode:: csharp

    // Create the tenant ID strategy for the client application.
    var tenantIdStrategy = new MyTenantIdentificationStrategy();

    // Register application-level dependencies.
    var builder = new ContainerBuilder();
    builder.RegisterType<BaseDependency>().As<IDependency>();

    // The service client is not different per tenant because
    // the service itself is multitenant - one client for all
    // the tenants and ***the service implementation*** switches.
    builder.Register(c =>
      new ChannelFactory<IMultitenantService>(
        new BasicHttpBinding(),
        new EndpointAddress("http://server/MultitenantService.svc"))).SingleInstance();

    // Register an endpoint behavior on the client channel factory that
    // will propagate the tenant ID across the wire in a message header.
    // In this example, the built-in TenantPropagationBehavior is used
    // to send a string-based tenant ID across the wire.
    builder.Register(c =>
      {
        var factory = c.Resolve<ChannelFactory<IMultitenantService>>();
        factory.Opening += (sender, args) => factory.Endpoint.Behaviors.Add(new TenantPropagationBehavior<string>(tenantIdStrategy));
        return factory.CreateChannel();
      });

    // Create the multitenant container.
    var mtc = new MultitenantContainer(tenantIdStrategy, builder.Build());

    // ... register tenant overrides, etc.

WCF Service Application Startup
"""""""""""""""""""""""""""""""

**In a WCF service application**, you register your defaults and tenant-specific overrides just as you normally would (:ref:`register_dependencies`) but you have to also:

- Set up the behavior for service hosts to expect an incoming tenant ID header (:ref:`behavior_id`) for tenant identification.
- Set the service host factory container to a ``MultitenantContainer``.

In the example below, **we are using the Autofac.Extras.Multitenant.Wcf.AutofacHostFactory** rather than the standard Autofac host factory (as outlined earlier).

.. sourcecode:: csharp

    namespace MultitenantExample.WcfService
    {
      public class Global : System.Web.HttpApplication
      {
        protected void Application_Start(object sender, EventArgs e)
        {
          // Create the tenant ID strategy.
          var tenantIdStrategy = new OperationContextTenantIdentificationStrategy();

          // Register application-level dependencies and service implementations.
          var builder = new ContainerBuilder();
          builder.RegisterType<BaseImplementation>().As<IMultitenantService>();
          builder.RegisterType<BaseDependency>().As<IDependency>();

          // Create the multitenant container.
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
            });

          // Add a behavior to service hosts that get created so incoming messages
          // get inspected and the tenant ID can be parsed from message headers.
          AutofacHostFactory.HostConfigurationAction =
            host =>
              host.Opening += (s, args) =>
                host.Description.Behaviors.Add(new TenantPropagationBehavior<string>(tenantIdStrategy));

          // Set the service implementation strategy to multitenant.
          AutofacHostFactory.ServiceImplementationDataProvider =
            new MultitenantServiceImplementationDataProvider();

          // Finally, set the host factory application container on the multitenant
          // WCF host to a multitenant container.
          AutofacHostFactory.Container = mtc;
        }
      }
    }
