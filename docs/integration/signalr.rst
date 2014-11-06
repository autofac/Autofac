=======
SignalR
=======

SignalR integration requires the `Autofac.SignalR NuGet package <https://nuget.org/packages/Autofac.SignalR/>`_.

SignalR integration provides dependency injection integration for SignalR hubs. **Due to SignalR internals, there is no support in SignalR for per-request lifetime dependencies.**

.. contents::
  :local:

Quick Start
===========
To get Autofac integrated with SignalR you need to reference the SignalR integration NuGet package, register your hubs, and set the dependency resolver.

.. sourcecode:: csharp

    protected void Application_Start()
    {
      var builder = new ContainerBuilder();

      // Register your SignalR hubs.
      builder.RegisterHubs(Assembly.GetExecutingAssembly());

      // Set the dependency resolver to be Autofac.
      var container = builder.Build();
      GlobalHost.DependencyResolver = new AutofacDependencyResolver(container);
    }

The sections below go into further detail about what each of these features do and how to use them.

Register Hubs
=============

At application startup, while building your Autofac container, you should register your SignalR hubs and their dependencies. This typically happens in an OWIN startup class or in the ``Application_Start`` method in ``Global.asax``.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // You can register hubs all at once using assembly scanning...
    builder.RegisterHubs(Assembly.GetExecutingAssembly());

    // ...or you can register individual hubs manually.
    builder.RegisterType<ChatHub>().ExternallyOwned();

If you register individual hubs, make sure they are registered as ``ExternallyOwned()``. This ensures SignalR is allowed to control disposal of the hubs rather than Autofac.

Set the Dependency Resolver
===========================

After building your container pass it into a new instance of the ``AutofacDependencyResolver`` class. Attach the new resolver to your ``GlobalHost.DependencyResolver`` (or ``HubConfiguration.Resolver`` if you're using OWIN) to let SignalR know that it should locate services using the ``AutofacDependencyResolver``. This is Autofac's implementation of the ``IDependencyResolver`` interface.

.. sourcecode:: csharp

    var container = builder.Build();
    GlobalHost.DependencyResolver = new AutofacDependencyResolver(container);

Managing Dependency Lifetimes
=============================

Given there is no support for per-request dependencies, **all dependencies resolved for SignalR hubs come from the root container**.

- If you have ``IDisposable`` components, they will live for the lifetime of the application because Autofac will :doc:`hold them until the lifetime scope/container is disposed <../lifetime/disposal>`. You should register these as ``ExternallyOwned()``.
- Any components registered as ``InstancePerLifetimeScope()`` will effectively be singletons. Given there's one root lifetime scope, you'll only get the one instance.

To make managing your hub dependency lifetimes easier you can have the root lifetime scope injected into the constructor of your hub. Next, create a child lifetime scope that you can use for the duration of your hub invocation and resolve the required services. Finally, make sure you dispose the child lifetime when the hub is disposed by SignalR. (This is similar to service location, but it's the only way to get a "per-hub" sort of scope. No, it's not awesome.)

.. sourcecode:: csharp

    public class MyHub : Hub
    {
      private readonly ILifetimeScope _hubLifetimeScope;
      private readonly ILogger _logger;

      public MyHub(ILifetimeScope lifetimeScope)
      {
        // Create a lifetime scope for the hub.
        _hubLifetimeScope = lifetimeScope.BeginLifetimeScope();

        // Resolve dependencies from the hub lifetime scope.
        _logger = _hubLifetimeScope.Resolve<ILogger>();
      }

      public void Send(string message)
      {
        // You can use your dependency field here!
        _logger.Write("Received message: " + message);

        Clients.All.addMessage(message);
      }

      protected override void Dispose(bool disposing)
      {
        // Dipose the hub lifetime scope when the hub is disposed.
        if (disposing && _hubLifetimeScope != null)
        {
          _hubLifetimeScope.Dispose();
        }

        base.Dispose(disposing);
      }
    }

If this is a common pattern in your application, you might consider creating a base/abstract hub from which other hubs can derive to save all the copy/paste creation/disposal of scopes.

OWIN Integration
================

If you are using SignalR :doc:`as part of an OWIN application <owin>`, you need to:

* Do all the stuff for standard SignalR integration - register controllers, set the dependency resolver, etc.
* Set up your app with the :doc:`base Autofac OWIN integration <owin>`.

.. sourcecode:: csharp

    public class Startup
    {
      public void Configuration(IAppBuilder app)
      {
        var builder = new ContainerBuilder();

        // STANDARD SIGNALR SETUP:

        // Get your HubConfiguration. In OWIN, you'll create one
        // rather than using GlobalHost.
        var config = new HubConfiguration();

        // Register your SignalR hubs.
        builder.RegisterHubs(Assembly.GetExecutingAssembly());

        // Set the dependency resolver to be Autofac.
        var container = builder.Build();
        config.Resolver = new AutofacDependencyResolver(container);

        // OWIN SIGNALR SETUP:

        // Register the Autofac middleware FIRST, then the standard SignalR middleware.
        app.UseAutofacMiddleware(container);
        app.MapSignalR("/signalr", config);
      }
    }

A common error in OWIN integration is use of the ``GlobalHost``. **In OWIN you create the configuration from scratch.** You should not reference ``GlobalHost`` anywhere when using the OWIN integration.
