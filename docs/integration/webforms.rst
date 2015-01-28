=========
Web Forms
=========

ASP.NET web forms integration requires the `Autofac.Web NuGet package <http://www.nuget.org/packages/Autofac.Web/>`_.

Web forms integration provides dependency injection integration for code-behind classes. It also adds :doc:`per-request lifetime support <../faq/per-request-scope>`.

.. contents::
  :local:

Quick Start
===========
To get Autofac integrated with web forms you need to reference the web forms integration NuGet package, add the modules to ``web.config``, and implement ``IContainerProviderAccessor`` on your ``Global`` application class.

Add the modules to ``web.config``:

.. sourcecode:: xml

    <configuration>
      <system.web>
        <httpModules>
          <!-- This section is used for IIS6 -->
          <add
            name="ContainerDisposal"
            type="Autofac.Integration.Web.ContainerDisposalModule, Autofac.Integration.Web"/>
          <add
            name="PropertyInjection"
            type="Autofac.Integration.Web.Forms.PropertyInjectionModule, Autofac.Integration.Web"/>
        </httpModules>
      </system.web>
      <system.webServer>
        <!-- This section is used for IIS7 -->
        <modules>
          <add
            name="ContainerDisposal"
            type="Autofac.Integration.Web.ContainerDisposalModule, Autofac.Integration.Web"
            preCondition="managedHandler"/>
          <add
            name="PropertyInjection"
            type="Autofac.Integration.Web.Forms.PropertyInjectionModule, Autofac.Integration.Web"
            preCondition="managedHandler"/>
        </modules>
      </system.webServer>
    </configuration>

Implement ``IContainerProviderAccessor``:

.. sourcecode:: csharp

    public class Global : HttpApplication, IContainerProviderAccessor
    {
      // Provider that holds the application container.
      static IContainerProvider _containerProvider;

      // Instance property that will be used by Autofac HttpModules
      // to resolve and inject dependencies.
      public IContainerProvider ContainerProvider
      {
        get { return _containerProvider; }
      }

      protected void Application_Start(object sender, EventArgs e)
      {
        // Build up your application container and register your dependencies.
        var builder = new ContainerBuilder();
        builder.RegisterType<SomeDependency>();
        // ... continue registering dependencies...

        // Once you're done registering things, set the container
        // provider up with your registrations.
        _containerProvider = new ContainerProvider(builder.Build());
      }
    }

The sections below go into further detail about what each of these features do and how to use them.

Add Modules to Web.config
=========================

The way that Autofac manages component lifetimes and adds dependency injection into the ASP.NET pipeline is through the use of `IHttpModule <http://msdn.microsoft.com/en-us/library/system.web.ihttpmodule.aspx>`_ implementations. You need to configure these modules in ``web.config``.

The following snippet config shows the modules configured.

.. sourcecode:: xml

    <configuration>
      <system.web>
        <httpModules>
          <!-- This section is used for IIS6 -->
          <add
            name="ContainerDisposal"
            type="Autofac.Integration.Web.ContainerDisposalModule, Autofac.Integration.Web"/>
          <add
            name="PropertyInjection"
            type="Autofac.Integration.Web.Forms.PropertyInjectionModule, Autofac.Integration.Web"/>
        </httpModules>
      </system.web>
      <system.webServer>
        <!-- This section is used for IIS7 -->
        <modules>
          <add
            name="ContainerDisposal"
            type="Autofac.Integration.Web.ContainerDisposalModule, Autofac.Integration.Web"
            preCondition="managedHandler"/>
          <add
            name="PropertyInjection"
            type="Autofac.Integration.Web.Forms.PropertyInjectionModule, Autofac.Integration.Web"
            preCondition="managedHandler"/>
        </modules>
      </system.webServer>
    </configuration>

Note that while there are two different sections the modules appear in - one each for IIS6 and IIS7 - **it is recommended that you have both in place**. The ASP.NET Developer Server uses the IIS6 settings even if your target deployment environment is IIS7. If you use IIS Express it will use the IIS7 settings.

The modules you see there do some interesting things:

- **The ContainerDisposalModule** lets Autofac dispose of any components created during request processing as soon as the request completes.
- **The PropertyInjectionModule** injects dependencies into pages before the page lifecycle executes. An alternative ``UnsetPropertyInjectionModule`` is also provided which will only set properties on web forms/controls that have null values. (Use only one or the other, but not both.)

Implement IContainerProviderAccessor in Global.asax
===================================================

The dependency injection modules expect that the ``HttpApplication`` instance supports ``IContainerProviderAccessor``. A complete global application class is shown below:

.. sourcecode:: csharp

    public class Global : HttpApplication, IContainerProviderAccessor
    {
      // Provider that holds the application container.
      static IContainerProvider _containerProvider;

      // Instance property that will be used by Autofac HttpModules
      // to resolve and inject dependencies.
      public IContainerProvider ContainerProvider
      {
        get { return _containerProvider; }
      }

      protected void Application_Start(object sender, EventArgs e)
      {
        // Build up your application container and register your dependencies.
        var builder = new ContainerBuilder();
        builder.RegisterType<SomeDependency>();
        // ... continue registering dependencies...

        // Once you're done registering things, set the container
        // provider up with your registrations.
        _containerProvider = new ContainerProvider(builder.Build());
      }
    }

``Autofac.Integration.Web.IContainerProvider`` exposes two useful properties: ``ApplicationContainer`` and ``RequestLifetime``.

- ``ApplicationContainer`` is the root container that was built at application start-up.
- ``RequestLifetime`` is a component :doc:`lifetime scope <../lifetime/index>` based on the application container that will be disposed of at the end of the current web request. It can be used whenever manual dependency resolution/service lookup is required. The components that it contains (apart from any singletons) will be specific to the current request (this is where :doc:`per-request lifetime dependencies <../faq/per-request-scope>` are resolved).

Tips and Tricks
===============

Structuring Pages and User Controls for DI
------------------------------------------

In order to inject dependencies into web forms pages (``System.Web.UI.Page`` instances) or user controls (``System.Web.UI.UserControl`` instances) **you must expose their dependencies as public properties that allow setting**. This enables the ``PropertyInjectionModule`` to populate those properties for you.

Be sure to register the dependencies you'll need at application startup.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<Component>().As<IService>().InstancePerRequest();
    // ... continue registering dependencies and then build the
    // container provider...
    _containerProvider = new ContainerProvider(builder.Build());

Then in your page codebehind, create public get/set properties for the dependencies you'll need:

.. sourcecode:: csharp

    // MyPage.aspx.cs
    public partial class MyPage : Page
    {
      // This property will be set for you by the PropertyInjectionModule.
      public IService MyService { get; set; }

      protected void Page_Load(object sender, EventArgs e)
      {
        // Now you can use the property that was set for you.
        label1.Text = this.MyService.GetMessage();
      }
    }

This same process of public property injection will work for user controls, too - just register the components at application startup and provide public get/set properties for the dependencies.

It is important to note **in the case of user controls that properties will only be automatically injected if the control is created and added to the page's Controls collection by the PreLoad step of the page request lifecycle**. Controls created dynamically either in code or through templates like the Repeater will not be visible at this point and must have their properties manually injected.

Manual Property Injection
-------------------------

In some cases, like in programmatic creation of user controls or other objects, you may need to manually inject properties on an object. To do this, you need to:

- Get the current application instance.
- Cast it to ``Autofac.Integration.Web.IContainerProviderAccessor``.
- Get the container provider from the application instance.
- Get the ``RequestLifetime`` from the ``IContainerProvider`` and use the ``InjectProperties()`` method to inject the properties on the object.

In code, that looks like this:

.. sourcecode:: csharp

    var cpa = (IContainerProviderAccessor)HttpContext.Current.ApplicationInstance;
    var cp = cpa.ContainerProvider;
    cp.RequestLifetime.InjectProperties(objectToSet);

Note you need both the ``Autofac`` and ``Autofac.Integration.Web`` namespaces in there to make property injection work because ``InjectProperties()`` is an extension method in the ``Autofac`` namespace.

Explicit Injection via Attributes
---------------------------------

When adding dependency injection to an existing application, it is sometimes desirable to distinguish between web forms pages that will have their dependencies injected and those that will not. The ``InjectPropertiesAttribute`` in ``Autofac.Integration.Web``, coupled with the ``AttributedInjectionModule`` help to achieve this.

**If you choose to use the AttributedInjectionModule, no dependencies will be automatically injected into public properties unless they're marked with a special attribute.**

First, remove the ``PropertyInjectionModule`` from your ``web.config`` file and replace it with the ``AttributedInjectionModule``:

.. sourcecode:: xml

    <configuration>
      <system.web>
        <httpModules>
          <!-- This section is used for IIS6 -->
          <add
            name="ContainerDisposal"
            type="Autofac.Integration.Web.ContainerDisposalModule, Autofac.Integration.Web"/>
          <add
            name="AttributedInjection"
            type="Autofac.Integration.Web.Forms.AttributedInjectionModule, Autofac.Integration.Web"/>
        </httpModules>
      </system.web>
      <system.webServer>
        <!-- This section is used for IIS7 -->
        <modules>
          <add
            name="ContainerDisposal"
            type="Autofac.Integration.Web.ContainerDisposalModule, Autofac.Integration.Web"
            preCondition="managedHandler"/>
          <add
            name="AttributedInjection"
            type="Autofac.Integration.Web.Forms.AttributedInjectionModule, Autofac.Integration.Web"
            preCondition="managedHandler"/>
        </modules>
      </system.webServer>
    </configuration>

Once this is in place, pages and controls will not have their dependencies injected by default. Instead, they must be marked with the ``Autofac.Integration.Web.Forms.InjectPropertiesAttribute`` or ``Autofac.Integration.Web.Forms.InjectUnsetPropertiesAttribute``. The difference:

- ``InjectPropertiesAttribute`` will always set public properties on the page/control if there are associated components registered with Autofac.
- ``InjectUnsetPropertiesAttribute`` will only set the public properties on the page/control if they are null and the associated components are registered.

.. sourcecode:: csharp

    [InjectProperties]
    public partial class MyPage : Page
    {
      // This property will be set for you by the AttributedInjectionModule.
      public IService MyService { get; set; }

      // ...use the property later as needed.
    }

Dependency Injection via Base Page Class
----------------------------------------

If you would rather not automatically inject properties using a module (e.g., the ``AttributedInjectionModule`` or ``PropertyInjectionModule`` as mentioned earlier), you can integrate Autofac in a more manual manner by creating a base page class that does manual property injection during the ``PreInit`` phase of the page request lifecycle.

This option allows you to derive pages that require dependency injection from a common base page class. Doing this may be desirable if you have only a very few pages that require dependency injection and you don't want the ``AttributedInjectionModule`` in the pipeline. (You still need the ``ContainerDisposalModule``.) If you have more than a small few pages it may be beneficial to consider explicit injection via attributes.

.. sourcecode:: csharp

    protected void Page_PreInit(object sender, EventArgs e)
    {
      var cpa = (IContainerProviderAccessor)HttpContext.Current.ApplicationInstance;
      var cp = cpa.ContainerProvider;
      cp.RequestLifetime.InjectProperties(this);
    }

Custom Dependency Injection Modules
-----------------------------------

If the provided *Property*, *Unset Property*, and *Attributed* dependency injection models are unsuitable, it is very easy to create a custom injection behavior. Simply subclass ``Autofac.Integration.Web.DependencyInjectionModule`` and use the result in ``Web.config``.

There is one abstract member to implement:

.. sourcecode:: csharp

    protected abstract IInjectionBehaviour GetInjectionBehaviourForHandlerType(Type handlerType);

The returned ``IInjectionBehaviour`` can be one of the predefined ``NoInjection``, ``PropertyInjection``, or ``UnsetPropertyInjection`` properties; or a custom implementation of the ``IInjectionBehaviour`` interface.