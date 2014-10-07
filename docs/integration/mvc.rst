===
MVC
===

Autofac is always kept up to date to support the latest version of ASP.NET MVC, so documentation is also kept up with the latest. Generally speaking, the integration remains fairly consistent across versions.

MVC integration requires the `Autofac.Mvc5 NuGet package <http://www.nuget.org/packages/Autofac.Mvc5/>`_.

MVC integration provides dependency injection integration for controllers, model binders, action filters, and views. It also adds :doc:`per-request lifetime support <../faq/per-request-scope>`.

.. contents::
  :local:

Quick Start
===========
To get Autofac integrated with MVC you need to reference the MVC integration NuGet package, register your controllers, and set the dependency resolver. You can optionally enable other features as well.

.. sourcecode:: csharp

    protected void Application_Start()
    {
      var builder = new ContainerBuilder();

      // Register your MVC controllers.
      builder.RegisterControllers(typeof(MvcApplication).Assembly);

      // OPTIONAL: Register model binders that require DI.
      builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
      builder.RegisterModelBinderProvider();

      // OPTIONAL: Register web abstractions like HttpContextBase.
      builder.RegisterModule<AutofacWebTypesModule>();

      // OPTIONAL: Enable property injection in view pages.
      builder.RegisterSource(new ViewRegistrationSource());

      // OPTIONAL: Enable property injection into action filters.
      builder.RegisterFilterProvider();

      // Set the dependency resolver to be Autofac.
      var container = builder.Build();
      DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
    }

The sections below go into further detail about what each of these features do and how to use them.

Register Controllers
====================

At application startup, while building your Autofac container, you should register your MVC controllers and their dependencies. This typically happens in an OWIN startup class or in the ``Application_Start`` method in ``Global.asax``.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // You can register controllers all at once using assembly scanning...
    builder.RegisterControllers(typeof(MvcApplication).Assembly);

    // ...or you can register individual controlllers manually.
    builder.RegisterType<HomeController>().InstancePerRequest();

Note that ASP.NET MVC requests controllers by their concrete types, so registering them ``As<IController>()`` is incorrect. Also, if you register controllers manually and choose to specify lifetimes, you must register them as ``InstancePerDependency()`` or ``InstancePerRequest()`` - **ASP.NET MVC will throw an exception if you try to reuse a controller instance for multiple requests**.

Set the Dependency Resolver
===========================

After building your container pass it into a new instance of the ``AutofacDependencyResolver`` class. Use the static ``DependencyResolver.SetResolver`` method to let ASP.NET MVC know that it should locate services using the ``AutofacDependencyResolver``. This is Autofac's implementation of the ``IDependencyResolver`` interface.

.. sourcecode:: csharp

    var container = builder.Build();
    DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

Register Model Binders
======================

An optional step you can take is to enable dependency injection for model binders. Similar to controllers, model binders (classes that implement ``IModelBinder``) can be registered in the container at application startup. You can do this with the ``RegisterModelBinders()`` method. You must also remember to register the ``AutofacModelBinderProvider`` using the ``RegisterModelBinderProvider()`` extension method. This is Autofac's implementation of the ``IModelBinderProvider`` interface.

.. sourcecode:: csharp

    builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
    builder.RegisterModelBinderProvider();

Because the ``RegisterModelBinders()`` extension method uses assembly scanning to add the model binders you need to specify what type(s) the model binders (``IModelBinder`` implementations) are to be registered for.

This is done by using the ``Autofac.Integration.Mvc.ModelBinderTypeAttribute``, like so:

.. sourcecode:: csharp

    [ModelBinderType(typeof(string))]
    public class StringBinder : IModelBinder
    {
      public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
      {
        // Implementation here
      }
    }

Multiple instances of the ``ModelBinderTypeAttribute`` can be added to a class if it is to be registered for multiple types.

Register Web Abstractions
=========================

The MVC integration includes an Autofac module that will add :doc:`HTTP request lifetime scoped <../faq/per-request-scope>` registrations for the web abstraction classes. This will allow you to put the web abstraction as a dependency in your class and get the correct value injected at runtime.

The following abstract classes are included:

* ``HttpContextBase``
* ``HttpRequestBase``
* ``HttpResponseBase``
* ``HttpServerUtilityBase``
* ``HttpSessionStateBase``
* ``HttpApplicationStateBase``
* ``HttpBrowserCapabilitiesBase``
* ``HttpFileCollectionBase``
* ``RequestContext``
* ``HttpCachePolicyBase``
* ``VirtualPathProvider``
* ``UrlHelper``

To use these abstractions add the ``AutofacWebTypesModule`` to the container using the standard ``RegisterModule()`` method.

.. sourcecode:: csharp

    builder.RegisterModule<AutofacWebTypesModule>();

Enable Property Injection for View Pages
========================================

You can make :doc:`property injection <../register/prop-method-injection>` available to your MVC views by adding the ``ViewRegistrationSource`` to your ``ContainerBuilder`` before building the application container.

.. sourcecode:: csharp

    builder.RegisterSource(new ViewRegistrationSource());

Your view page must inherit from one of the base classes that MVC supports for creating views. When using the Razor view engine this will be the ``WebViewPage`` class.

.. sourcecode:: csharp

    public abstract class CustomViewPage : WebViewPage
    {
      public IDependency Dependency { get; set; }
    }

The ``ViewPage``, ``ViewMasterPage`` and ``ViewUserControl`` classes are supported when using the web forms view engine.

.. sourcecode:: csharp

    public abstract class CustomViewPage : ViewPage
    {
      public IDependency Dependency { get; set; }
    }

Ensure that your actual view page inherits from your custom base class. This can be achieved using the ``@inherits`` directive inside your ``.cshtml`` file for the Razor view engine::

    @inherits Example.Views.Shared.CustomViewPage

When using the web forms view engine you set the ``Inherits`` attribute on the ``@ Page`` directive inside your ``.aspx`` file instead.

.. sourcecode:: aspx-cs

    <%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="Example.Views.Shared.CustomViewPage"%>


Enable Property Injection for Action Filters
============================================

To make use of property injection for your filter attributes call the ``RegisterFilterProvider()`` method on the ``ContainerBuilder`` before building your container and providing it to the ``AutofacDependencyResolver``.

.. sourcecode:: csharp

    builder.RegisterFilterProvider();

This allows you to add properties to your filter attributes and any matching dependencies that are registered in the container will be injected into the properties.

For example, the action filter below will have the ``ILogger`` instance injected from the container (assuming you register an ``ILogger``. Note that **the attribute itself does not need to be registered in the container**.

.. sourcecode:: csharp

    public class CustomActionFilter : ActionFilterAttribute
    {
      public ILogger Logger { get; set; }

      public override void OnActionExecuting(ActionExecutingContext filterContext)
      {
        Logger.Log("OnActionExecuting");
      }
    }

The same simple approach applies to the other filter attribute types such as authorization attributes.

.. sourcecode:: csharp

    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
      public ILogger Logger { get; set; }

      protected override bool AuthorizeCore(HttpContextBase httpContext)
      {
        Logger.Log("AuthorizeCore");
        return true;
      }
    }

After applying the attributes to your actions as usual your work is done.

.. sourcecode:: csharp

    [CustomActionFilter]
    [CustomAuthorizeAttribute]
    public ActionResult Index()
    {
    }

OWIN Integration
================

If you are using MVC :doc:`as part of an OWIN application <owin>`, you need to:

* Do all the stuff for standard MVC integration - register controllers, set the dependency resolver, etc.
* Set up your app with the :doc:`base Autofac OWIN integration <owin>`.
* Add a reference to the `Autofac.Mvc5.Owin <http://www.nuget.org/packages/Autofac.Mvc5.Owin/>`_ NuGet package.
* In your application startup class, register the Autofac MVC middleware after registering the base Autofac middleware.

.. sourcecode:: csharp

    public class Startup
    {
      public void Configuration(IAppBuilder app)
      {
        var builder = new ContainerBuilder();

        // STANDARD MVC SETUP:

        // Register your MVC controllers.
        builder.RegisterControllers(typeof(MvcApplication).Assembly);

        // Run other optional steps, like registering model binders,
        // web abstractions, etc., then set the dependency resolver
        // to be Autofac.
        var container = builder.Build();
        DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

        // OWIN MVC SETUP:

        // Register the Autofac middleware FIRST, then the Autofac MVC middleware.
        app.UseAutofacMiddleware(container);
        app.UseAutofacMvc();
      }
    }

Using "Plugin" Assemblies
=========================

If you have controllers in a "plugin assembly" that isn't referenced by the main application `you'll need to register your controller plugin assembly with the ASP.NET BuildManager <http://www.paraesthesia.com/archive/2013/01/21/putting-controllers-in-plugin-assemblies-for-asp-net-mvc.aspx>`_.

You can do this through configuration or programmatically.

**If you choose configuration**, you need to add your plugin assembly to the ``/configuration/system.web/compilation/assemblies`` list. If your plugin assembly isn't in the ``bin`` folder, you also need to update the ``/configuration/runtime/assemblyBinding/probing`` path.

.. sourcecode:: xml

    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <runtime>
        <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
          <!--
              If you put your plugin in a folder that isn't bin, add it to the probing path
          -->
          <probing privatePath="bin;bin\plugins" />
        </assemblyBinding>
      </runtime>
      <system.web>
        <compilation>
          <assemblies>
            <add assembly="The.Name.Of.Your.Plugin.Assembly.Here" />
          </assemblies>
        </compilation>
      </system.web>
    </configuration>

**If you choose programmatic registration**, you need to do it during pre-application-start before the ASP.NET ``BuildManager`` kicks in.

Create an initializer class to do the assembly scanning/loading and registration with the ``BuildManager``:

.. sourcecode:: csharp

    using System.IO;
    using System.Reflection;
    using System.Web.Compilation;

    namespace MyNamespace
    {
      public static class Initializer
      {
        public static void Initialize()
        {
          var pluginFolder = new DirectoryInfo(HostingEnvironment.MapPath("~/plugins"));
          var pluginAssemblies = pluginFolder.GetFiles("*.dll", SearchOption.AllDirectories);
          foreach (var pluginAssemblyFile in pluginAssemblyFiles)
          {
            var asm = Assembly.LoadFrom(pluginAssemblyFile.FullName);
            BuildManager.AddReferencedAssembly(asm);
          }
        }
      }
    }

Then be sure to register your pre-application-start code with an assembly attribute:

.. sourcecode:: csharp

    [assembly: PreApplicationStartMethod(typeof(Initializer), "Initialize")]

Unit Testing
============

When unit testing an ASP.NET MVC app that uses Autofac where you have ``InstancePerRequest`` components registered, you'll get an exception when you try to resolve those components because there's no HTTP request lifetime during a unit test.

The :doc:`per-request lifetime scope <../faq/per-request-scope>` topic outlines strategies for testing and troubleshooting per-request-scope components.

Example Implementation
======================

`The Autofac source <https://github.com/autofac/Autofac>`_ contains a demo web application project called ``Remember.Web``. It demonstrates many of the aspects of MVC that Autofac is used to inject.
