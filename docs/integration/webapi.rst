=======
Web API
=======

Web API integration requires the `Autofac.WebApi NuGet package <https://www.nuget.org/packages/Autofac.WebApi/>`_.

Web API integration provides dependency injection integration for controllers, model binders, and action filters. It also adds :doc:`per-request lifetime support <../faq/per-request-scope>`.

.. contents::
  :local:

Quick Start
===========
To get Autofac integrated with Web API you need to reference the Web API integration NuGet package, register your controllers, and set the dependency resolver. You can optionally enable other features as well.

.. sourcecode:: csharp

    protected void Application_Start()
    {
      var builder = new ContainerBuilder();

      // Get your HttpConfiguration.
      var config = GlobalConfiguration.Configuration;

      // Register your Web API controllers.
      builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

      // OPTIONAL: Register the Autofac filter provider.
      builder.RegisterWebApiFilterProvider(config);

      // Set the dependency resolver to be Autofac.
      var container = builder.Build();
      config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
    }

The sections below go into further detail about what each of these features do and how to use them.

Get the HttpConfiguration
=========================

In Web API, setting up the application requires you to update properties and set values on an ``HttpConfiguration`` object. Depending on how you're hosting your application, where you get this configuration may be different. Through the documentation, we'll refer to "your ``HttpConfiguration``" and you'll need to make a choice of how to get it.

For standard IIS hosting, the ``HttpConfiguration`` is ``GlobalConfiguration.Configuration``.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    var config = GlobalConfiguration.Configuration;
    builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
    var container = builder.Build();
    config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

For self hosting, the ``HttpConfiguration`` is your ``HttpSelfHostConfiguration`` instance.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    var config = new HttpSelfHostConfiguration("http://localhost:8080");
    builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
    var container = builder.Build();
    config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

For OWIN integration, the ``HttpConfiguration`` is the one you create in your app startup class and pass to the Web API middleware.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    var config = new HttpConfiguration();
    builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
    var container = builder.Build();
    config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

Register Controllers
====================

At application startup, while building your Autofac container, you should register your Web API controllers and their dependencies. This typically happens in an OWIN startup class or in the ``Application_Start`` method in ``Global.asax``.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // You can register controllers all at once using assembly scanning...
    builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

    // ...or you can register individual controlllers manually.
    builder.RegisterType<ValuesController>().InstancePerRequest();

Set the Dependency Resolver
===========================

After building your container pass it into a new instance of the ``AutofacWebApiDependencyResolver`` class. Attach the new resolver to your ``HttpConfiguration.DependencyResolver`` to let Web API know that it should locate services using the ``AutofacWebApiDependencyResolver``. This is Autofac's implementation of the ``IDependencyResolver`` interface.

.. sourcecode:: csharp

    var container = builder.Build();
    config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

Provide Filters via Dependency Injection
========================================
Because attributes are created via the reflection API you don't get to call the constructor yourself. That leaves you with no other option except for property injection when working with attributes. The Autofac integration with Web API provides a mechanism that allows you to create classes that implement the filter interfaces (``IAutofacActionFilter``, ``IAutofacAuthorizationFilter`` and ``IAutofacExceptionFilter``) and wire them up to the desired controller or action method using the registration syntax on the container builder.

Register the Filter Provider
----------------------------

You need to register the Autofac filter provider implementation because it is does the work of wiring up the filter based on the registration. This is done by calling the ``RegisterWebApiFilterProvider`` method on the container builder and providing an ``HttpConfiguration`` instance.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterWebApiFilterProvider(config);

Implement the Filter Interface
------------------------------

Instead of deriving from one of the existing Web API filter attributes your class implements the appropriate filter interface defined in the integration. The filter below is an action filter and  implements ``IAutofacActionFilter`` instead of ``System.Web.Http.Filters.IActionFilter``.

.. sourcecode:: csharp

    public class LoggingActionFilter : IAutofacActionFilter
    {
      readonly ILogger _logger;

      public LoggingActionFilter(ILogger logger)
      {
        _logger = logger;
      }

      public void OnActionExecuting(HttpActionContext actionContext)
      {
        _logger.Write(actionContext.ActionDescriptor.ActionName);
      }

      public void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
      {
        _logger.Write(actionExecutedContext.ActionContext.ActionDescriptor.ActionName);
      }
    }

Register the Filter
-------------------

For the filter to execute you need to register it with the container and inform it which controller, and optionally action, should be targeted. This is done using the ``AsActionFilterFor()``, ``AsAuthorizationFilterFor()`` and ``AsExceptionFilterFor()`` extension methods on the ``ContainerBuilder``.

These methods require a generic type parameter for the type of the controller, and an optional lambda expression that indicates a specific method on the controller the filter should be applied to. If you don’t provide the lambda expression the filter will be applied to all action methods on the controller in the same way that placing an attribute based filter at the controller level would.

In the example below the filter is being applied to the ``Get`` action method on the ``ValuesController``.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
     
    builder.Register(c => new LoggingActionFilter(c.Resolve<ILogger>()))
        .AsWebApiActionFilterFor<ValuesController>(c => c.Get(default(int)))
        .InstancePerApiRequest();

When applying the filter to an action method that requires a parameter use the ``default`` keyword with the data type of the parameter as a placeholder in your lambda expression. For example, the ``Get`` action method in the example above required an ``int`` parameter and used ``efault(int)`` as a strongly-typed placeholder in the lambda expression.

It is also possible to provide a base controller type in the generic type parameter to have the filter applied to all derived controllers. In addition, you can also make your lambda expression for the action method target a method on a base controller type and have it applied to that method on all derived controllers.

Why We Use Non-Standard Filter Interfaces
-----------------------------------------

If you are wondering why special interfaces were introduced this should become more apparent if you take a look at the signature of the ``IActionFilter`` interface in Web API.

.. sourcecode:: csharp

    public interface IActionFilter : IFilter
    {
      Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation);
    }

Now compare that to the Autofac interface that you need to implement instead.

.. sourcecode:: csharp

    public interface IAutofacActionFilter
    {
      void OnActionExecuting(HttpActionContext actionContext);

      void OnActionExecuted(HttpActionExecutedContext actionExecutedContext);
    }

The problem is that the ``OnActionExecuting`` and ``OnActionExecuted`` methods are actually defined on the the ``ActionFilterAttribute`` and not on the ``IActionFilter`` interface. Extensive use of the ``System.Threading.Tasks`` namespace in Web API means that chaining the return task along with the appropriate error handling in the attribute actually requires a significant amount of code (the ``ActionFilterAttribute`` contains nearly 100 lines of code for that). This is definitely not something that you want to be handling yourself.

Autofac introduces the new interfaces to allow you to concentrate on implementing the code for your filter and not all that plumbing. Internally it creates custom instances of the actual Web API attributes that resolve the filter implementations from the container and execute them at the appropriate time.

Another reason for creating the internal attribute wrappers is to support the ``InstancePerRequest`` lifetime scope for filters. See below for more on that.

Standard Web API Filters are Singletons
---------------------------------------

You may notice that if you use the standard Web API filters that you can't use ``InstancePerRequest`` dependencies.

Unlike the filter provider in :doc:`MVC <mvc>`, the one in Web API does not allow you to specify that the filter instances should not be cached. This means that **all filter attributes in Web API are effectively singleton instances that exist for the entire lifetime of the application.**

If you are trying to get per-request dependencies in a filter, you'll find that will only work if you use the Autofac filter interfaces. Using the standard Web API filters, the dependencies will be injected once - the first time the filter is resolved - and never again.

**If you are unable to use the Autofac interfaces and you need per-request or instance-per-dependency services in your filters, you must use service location.** Luckily, Web API makes getting the current request scope very easy - it comes right along with the ``HttpRequestMessage``.

Here's an example of a filter that uses service location with the Web API ``IDependencyScope`` to get per-request dependencies:

.. sourcecode:: csharp

    public interface ServiceCallActionFilterAttribute : ActionFilterAttribute
    {
      public override void OnActionExecuting(HttpActionContext actionContext)
      {
        // Get the request lifetime scope so you can resolve services.
        var requestScope = actionContext.Request.GetDependencyScope();

        // Resolve the service you want to use.
        var service = requestScope.GetService(typeof(IMyService)) as IMyService;

        // Do the rest of the work in the filter.
        service.DoWork();
      }
    }

Per-Controller-Type Services
============================

Web API has an interesting feature that allows you to configure the set of Web API services (those such as ``IActionValueBinder``) that should be used per-controller-type by adding an attribute that implements the ``IControllerConfiguration`` interface to your controller.

Through the ``Services`` property on the ``HttpControllerSettings`` parameter passed to the ``IControllerConfiguration.Initialize`` method you can override the global set of services. This attribute-based approach seems to encourage you to directly instantiate service instances and then override the ones registered globally. Autofac allows these per-controller-type services to be configured through the container instead of being buried away in an attribute without dependency injection support.

Add the Controller Configuration Attribute
------------------------------------------

There is no escaping adding an attribute to the controller that the configuration should be applied to because that is the extension point defined by Web API. The Autofac integration includes an ``AutofacControllerConfigurationAttribute`` that you can apply to your Web API controllers to indicate that they require per-controller-type configuration.

The point to remember here is that **the actual configuration of what services should be applied will be done when you build your container** and there is no need to implement any of that in an actual attribute. In this case, the attribute can be considered as purely a marker that indicates that the container will define the configuration information and provide the service instances.

.. sourcecode:: csharp

    [AutofacControllerConfiguration]
    public class ValuesController : ApiController
    {
      // Implementation...
    }

Supported Services
------------------

The supported services can be divided into single-style or multiple-style services. For example, you can only have one ``IHttpActionInvoker`` but you can have multiple ``ModelBinderProvider`` services.

You can use dependency injection for the following single-style services:

- ``IHttpActionInvoker``
- ``HttpActionSelector``
- ``ActionValueBinder``
- ``IBodyModelValidator``
- ``IContentNegotiator``
- ``IHttpControllerActivator``
- ``ModelMetadataProvider``

The following multiple style services are supported:

- ``ModelBinderProvider``
- ``ModelValidatorProvider``
- ``ValueProviderFactory``
- ``MediaTypeFormatter``

In the list of the multiple-style services above the ``MediaTypeFormatter`` is actually the odd one out. Technically, it isn't actually a service and is added to the ``MediaTypeFormatterCollection`` on the ``HttpControllerSettings`` instance and not the ``ControllerServices`` container. We figred that there was no reason to exclude ``MediaTypeFormatter`` instances from dependency injection support and made sure that they could be resolved from the container per-controller type, too.

Service Registration
--------------------

Here is an example of registering a custom ``IHttpActionSelector`` implementation as ``InstancePerApiControllerType()`` for the ``ValuesController``. When applied to a controller type all derived controllers will also receive the same configuration; the ``AutofacControllerConfigurationAttribute`` is inherited by derived controller types and the same behavior applies to the registrations in the container. When you register a single-style service it will always replace the default service configured at the global level.

.. sourcecode:: csharp

    builder.Register(c => new CustomActionSelector())
           .As<IHttpActionSelector>()
           .InstancePerApiControllerType(typeof(ValuesController));

Clearing Existing Services
--------------------------

By default, multiple-style services are appended to the existing set of services configured at the global level. When registering multiple-style services with the container you can choose to clear the existing set of services so that only the ones you have registered as ``InstancePerApiControllerType()`` will be used. This is done by setting the ``clearExistingServices`` parameter to ``true`` on the ``InstancePerApiControllerType()`` method. Existing services of that type will be removed if any of the registrations for the multiple-style service indicate that they want that to happen.

.. sourcecode:: csharp

    builder.Register(c => new CustomModelBinderProvider())
           .As<ModelBinderProvider>()
           .InstancePerApiControllerType(
              typeof(ValuesController),
              clearExistingServices: true);

Per-Controller-Type Service Limitations
---------------------------------------

If you are using per-controller-type services, it is not possible to take dependencies on other services that are registered as ``InstancePerRequest()``. The problem is that Web API is caching these services and is not requesting them from the container each time a controller of that type is created. It is most likely not possible for Web API to easily add that support that without introducing the notion of a key (for the controller type) into the DI integration, which would mean that all containers would need to support keyed services.

OWIN Integration
================

If you are using Web API :doc:`as part of an OWIN application <owin>`, you need to:

* Do all the stuff for standard Web API integration - register controllers, set the dependency resolver, etc.
* Set up your app with the :doc:`base Autofac OWIN integration <owin>`.
* Add a reference to the `Autofac.WebApi2.Owin <http://www.nuget.org/packages/Autofac.WebApi2.Owin/>`_ NuGet package.
* In your application startup class, register the Autofac Web API middleware after registering the base Autofac middleware.

.. sourcecode:: csharp

    public class Startup
    {
      public void Configuration(IAppBuilder app)
      {
        var builder = new ContainerBuilder();

        // STANDARD WEB API SETUP:

        // Get your HttpConfiguration. In OWIN, you'll create one
        // rather than using GlobalConfiguration.
        var config = new HttpConfiguration();

        // Register your Web API controllers.
        builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

        // Run other optional steps, like registering filters,
        // per-controller-type services, etc., then set the dependency resolver
        // to be Autofac.
        var container = builder.Build();
        config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

        // OWIN WEB API SETUP:

        // Register the Autofac middleware FIRST, then the Autofac Web API middleware,
        // and finally the standard Web API middleware.
        app.UseAutofacMiddleware(container);
        app.UseAutofacWebApi(config);
        app.UseWebApi(config);
      }
    }

A common error in OWIN integration is use of the ``GlobalConfiguration.Configuration``. **In OWIN you create the configuration from scratch.** You should not reference ``GlobalConfiguration.Configuration`` anywhere when using the OWIN integration.

Unit Testing
============

When unit testing an ASP.NET Web API app that uses Autofac where you have ``InstancePerRequest`` components registered, you'll get an exception when you try to resolve those components because there's no HTTP request lifetime during a unit test.

The :doc:`per-request lifetime scope <../faq/per-request-scope>` topic outlines strategies for testing and troubleshooting per-request-scope components.
