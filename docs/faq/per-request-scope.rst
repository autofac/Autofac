==============================================
How do I work with per-request lifetime scope?
==============================================

In applications that have a request/response semantic (e.g., :doc:`ASP.NET MVC <../integration/mvc>` or :doc:`Web API <../integration/webapi>`), you can register dependencies to be "instance-per-request," meaning you will get a one instance of the given dependency for each request handled by the application and that instance will be tied to the individual request lifecycle.

In order to understand per-request lifetime, you should have a good general understanding of :doc:`how dependency lifetime scopes work in general <../lifetime/instance-scope>`. Once you understand how dependency lifetime scopes work, per-request lifetime scope is easy.

.. contents::
  :local:


Registering Dependencies as Per-Request
=======================================

When you want a dependency registered as per-request, use the ``InstancePerRequest()`` registration extension:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<ConsoleLogger>()
           .As<ILogger>()
           .InstancePerRequest();
    var container = builder.Build();

You'll get a new instance of the component for every inbound request for your application. The handling of the creation of the request-level lifetime scope and the cleanup of that scope are generally dealt with via the :doc:`Autofac application integration libraries <../integration/index>` for your application type.


How Per-Request Lifetime Works
==============================

Per-request lifetime makes use of :doc:`tagged lifetime scopes and the "Instance Per Matching Lifetime Scope" mechanism <../lifetime/instance-scope>`.

:doc:`Autofac application integration libraries <../integration/index>` hook into different application types and, on an inbound request, they create a nested lifetime scope with a "tag" that identifies it as a request lifetime scope::

    +--------------------------+
    |    Autofac Container     |
    |                          |
    | +----------------------+ |
    | | Tagged Request Scope | |
    | +----------------------+ |
    +--------------------------+

When you register a component as ``InstancePerRequest()``, you're telling Autofac to look for a lifetime scope that is tagged as the request scope and to resolve the component from there. That way if you have unit-of-work lifetime scopes that take place during a single request, the per-request dependency will be shared during the request::

    +----------------------------------------------------+
    |                 Autofac Container                  |
    |                                                    |
    | +------------------------------------------------+ |
    | |              Tagged Request Scope              | |
    | |                                                | |
    | | +--------------------+  +--------------------+ | |
    | | | Unit of Work Scope |  | Unit of Work Scope | | |
    | | +--------------------+  +--------------------+ | |
    | +------------------------------------------------+ |
    +----------------------------------------------------+

The request scope is tagged with a constant value ``Autofac.Core.Lifetime.MatchingScopeLifetimeTags.AutofacWebRequest``, which equates to the string ``AutofacWebRequest``. If the request lifetime scope isn't found, you'll get a ``DependencyResolutionException`` that tells you the request lifetime scope isn't found.

There are tips on troubleshooting this exception below in the :ref:`troubleshooting` section.

.. _sharing-dependencies:

Sharing Dependencies Across Apps Without Requests
=================================================

A common situation you might see is that you have a single :doc:`Autofac module <../configuration/modules>` that performs some dependency registrations and you want to share that module between two applications - one that has a notion of per-request lifetime (like a :doc:`Web API <../integration/webapi>` application) and one that doesn't (like a console app or Windows Service).

**How do you register dependencies as per-request and allow registration sharing?**

There are a couple of potential solutions to this problem.

**Option 1**: Change your ``InstancePerRequest()`` registrations to be ``InstancePerLifetimeScope()``. *Most* applications don't create their own nested unit-of-work lifetime scopes; instead, the only real child lifetime scope that gets created *is the request lifetime*. If this is the case for your application, then ``InstancePerRequest()`` and ``InstancePerLifetimeScope()`` become effectively identical. You will get the same behavior. In the application that doesn't support per-request semantics, you can create child lifetime scopes as needed for component sharing.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // If your application does NOT create its own child
    // lifetime scopes anywhere, then change this...
    //
    // builder.RegisterType<ConsoleLogger>()
    //        .As<ILogger>()
    //        .InstancePerRequest();
    //
    // ..to this:
    builder.RegisterType<ConsoleLogger>()
           .As<ILogger>()
           .InstancePerLifetimeScope();
    var container = builder.Build();

**Option 2**: Set up your registration module to take a parameter and indicate which lifetime scope registration type to use.

.. sourcecode:: csharp

    public class LoggerModule : Module
    {
      private bool _perRequest;
      public LoggerModule(bool supportPerRequest)
      {
        this._perRequest = supportPerRequest;
      }

      protected override void Load(ContainerBuilder builder)
      {
        var reg = builder.RegisterType<ConsoleLogger>().As<ILogger>();
        if(this._perRequest)
        {
          reg.InstancePerRequest();
        }
        else
        {
          reg.InstancePerLifetimeScope();
        }
      }
    }

    // Register the module in each application and pass
    // an appropriate parameter indicating if the app supports
    // per-request or not, like this:
    // builder.RegisterModule(new LoggerModule(true));

**Option 3**: A third, but more complex, option is to implement custom per-request semantics in the application that doesn't naturally have these semantics. For example, a Windows Service doesn't necessarily have per-request semantics, but if it's self-hosting a custom service that takes requests and provides responses, you could add per-request lifetime scopes around each request and enable support of per-request dependencies. You can read more about this in the :ref:`custom-semantics` section.


.. _testing:

Testing with Per-Request Dependencies
=====================================

If you have an application that registers per-request dependencies, you may want to re-use the registration logic to set up dependencies in unit tests. Of course, you'll find that your unit tests don't have request lifetime scopes available, so you'll end up with a ``DependencyResolutionException`` that indicates the ``AutofacWebRequest`` scope can't be found. How do you use the registrations in a testing environment?

**Option 1**: Create some custom registrations for each specific test fixture. Particularly if you're in a unit test environment, you probably shouldn't be wiring up the whole real runtime environment for the test - you should have test doubles for all the external required dependencies instead. Consider mocking out the dependencies and not actually doing the full shared set of registrations in the unit test environment.

**Option 2**: Look at the choices for sharing registrations in the :ref:`sharing-dependencies` section. Your unit test could be considered "an application that doesn't support per-request registrations" so using a mechanism that allows sharing between application types might be appropriate.

**Option 3**: Implement a fake "request" in the test. The intent here would be that before the test runs, a real Autofac lifetime scope with the ``AutofacWebRequest`` label is created, the test is run, and then the fake "request" scope is disposed - as though a full request was actually run. This is a little more complex and the method differs based on application type.

Faking an MVC Request Scope
---------------------------

The :doc:`Autofac ASP.NET MVC integration <../integration/mvc>` uses an ``ILifetimeScopeProvider`` implementation along with the ``AutofacDependencyResolver`` to dynamically create a request scope as needed. To fake out the MVC request scope, you need to provide a test ``ILifetimeScopeProvider`` that doesn't involve the actual HTTP request. A simple version might look like this:

.. sourcecode:: csharp

    public class SimpleLifetimeScopeProvider : ILifetimeScopeProvider
    {
      private readonly IContainer _container;
      private ILifetimeScope _scope;

      public SimpleLifetimeScopeProvider(IContainer container)
      {
        this._container = container;
      }

      public ILifetimeScope ApplicationContainer
      {
        get { return this._container; }
      }

      public void EndLifetimeScope()
      {
        if (this._scope != null)
        {
          this._scope.Dispose();
          this._scope = null;
        }
      }

      public ILifetimeScope GetLifetimeScope(Action<ContainerBuilder> configurationAction)
      {
        if (this._scope == null)
        {
          this._scope = (configurationAction == null)
                 ? this.ApplicationContainer.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag)
                 : this.ApplicationContainer.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag, configurationAction);
        }

        return this._scope;
      }
    }

When creating your ``AutofacDependencyResolver`` from your built application container, you'd manually specify your simple lifetime scope provider. Make sure you set up the resolver before your test runs, then after the test runs you need to clean up the fake request scope. In NUnit, it'd look like this:

.. sourcecode:: csharp

    private IDependencyResolver _originalResolver = null;
    private ILifetimeScopeProvider _scopeProvider = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      // Build the container, then...
      this._scopeProvider = new SimpleLifetimeScopeProvider(container);
      var resolver = new AutofacDependencyResolver(container, provider);
      this._originalResolver = DependencyResolver.Current;
      DependencyResolver.SetResolver(resolver);
    }

    [TearDown]
    public void TearDown()
    {
      // Clean up the fake 'request' scope.
      this._scopeProvider.EndLifetimeScope();
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      // If you're mucking with statics, always put things
      // back the way you found them!
      DependencyResolver.SetResolver(this._originalResolver);
    }


Faking a Web API Request Scope
------------------------------

In Web API, the request lifetime scope is actually dragged around the system along with the inbound ``HttpRequestMessage`` as an ``ILifetimeScope`` object. To fake out a request scope, you just have to get the ``ILifetimeScope`` attached to the message you're processing as part of your test.

During test setup, you should build the dependency resolver as you would in the application and associate that with an ``HttpConfiguration`` object. In each test, you'll create the appropriate ``HttpRequestMessage`` to process based on the use case being tested, then use built-in Web API extension methods to attach the configuration to the message and get the request scope from the message.

In NUnit it'd look like this:

.. sourcecode:: csharp

    private HttpConfiguration _configuration = null;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      // Build the container, then...
      this._configuration = new HttpConfiguration
      {
        DependencyResolver = new AutofacWebApiDependencyResolver(container);
      }
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      // Clean up - automatically handles
      // cleaning up the dependency resolver.
      this._configuration.Dispose();
    }

    [Test]
    public void MyTest()
    {
      // Dispose of the HttpRequestMessage to dispose of the
      // request lifetime scope.
      using(var message = CreateTestHttpRequestMessage())
      {
        message.SetConfiguration(this._configuration);

        // Now do your test. Use the extension method
        // message.GetDependencyScope()
        // to get the request lifetime scope from Web API.
      }
    }

.. _troubleshooting:

Troubleshooting Per-Request Dependencies
========================================

There are a few gotchas when you're working with per-request dependencies. Here's some troubleshooting help.

No Scope with a Tag Matching 'AutofacWebRequest'
------------------------------------------------

A very common exception people see when they start working with per-request lifetime scope is:

``DependencyResolutionException: No scope with a Tag matching
'AutofacWebRequest' is visible from the scope in which the instance
was requested. This generally indicates that a component registered
as per-HTTP request is being requested by a SingleInstance()
component (or a similar scenario.) Under the web integration always
request dependencies from the DependencyResolver.Current or
ILifetimeScopeProvider.RequestLifetime, never from the container
itself.``

What this means is that the application tried to resolve a dependency that is registered as ``InstancePerRequest()`` but there wasn't any request lifetime in place.

Common causes for this include:

  * Application registrations are being shared across application types.
  * A unit test is running with real application registrations but isn't simulating per-request lifetimes.
  * Code is running during application startup (e.g., in an ASP.NET ``Global.asax``) that uses dependency resolution when there isn't an active request yet.
  * Code is running in a "background thread" (where there's no request semantics) but is trying to call the ASP.NET MVC ``DependencyResolver`` to do service location.

Tracking down the source of the issue can be troublesome. In many cases, you might look at what is being resolved and see that the component being resolved is *not registered as per-request* and the dependencies that component uses are also *not registered as per-request*. In cases like this, you may need to go all the way down the dependency chain. The exception could be coming from something deep in the dependency chain. Usually a close examination of the call stack can help you. In cases where you are doing :doc:`dynamic assembly scanning <../register/scanning>` to locate :doc:`modules <../configuration/modules>` to register, the source of the troublesome registration may not be immediately obvious.

Regardless, somewhere along the line, *something* is looking for a per-request lifetime scope and it's not being found.

If you are trying to share registrations across application types, check out the :ref:`sharing-dependencies` section.

If you are trying to unit test with per-request dependencies, the sections :ref:`testing` and :ref:`sharing-dependencies` can give you some tips.

If you have application startup code or a background thread in an ASP.NET MVC app trying to use ``DependencyResolver.Current`` - the ``AutofacDependencyResolver`` requires a web context to resolve things. When you try to resolve something from the resolver, it's going to try to spin up a per-request lifetime scope and store it along with the current ``HttpContext``. If there isn't a current context, things will fail. Accessing ``AutofacDependencyResolver.Current`` will not get you around that - the way the current resolver property works, it locates itself from the current web request scope. (It does this to allow working with applications like Glimpse and other instrumentation mechanisms.)

For application startup code or background threads, you may need to look at a different service locator mechanism like :doc:`Common Service Locator <../integration/csl>` to bypass the need for per-request scope. If you do that, you'll also need to check out the :ref:`sharing-dependencies` section to update your component registrations so they also don't necessarily require a per-request scope.


No Per-Request Filter Dependencies in Web API
---------------------------------------------

If you are using the :doc:`Web API integration <../integration/webapi>` and ``AutofacWebApiFilterProvider`` to do dependency injection into your action filters, you may notice that **dependencies in filters are resolved one time only and not on a per-request basis**.

This is a shortcoming in Web API. The Web API internals create filter instances and then cache them, never to be created again. This removes any "hooks" that might otherwise have existed to do anything on a per-request basis in a filter.

If you need to do something per-request in a filter, you will need to use service location and manually get the request lifetime scope from the context in your filter. For example, an ``ActionFilterAttribute`` might look like this:

.. sourcecode:: csharp

    public class LoggingFilterAttribute : ActionFilterAttribute
    {
      public override void OnActionExecuting(HttpActionContext context)
      {
        var logger = context.Request.GetDependencyScope().GetService(typeof(ILogger)) as ILogger;
        logger.Log("Executing action.");
      }
    }

Using this service location mechanism, you wouldn't even need the ``AutofacWebApiFilterProvider`` - you can do this even without using Autofac at all.


.. _custom-semantics:

Implementing Custom Per-Request Semantics
=========================================

You may have a custom application that handles requests - like a Windows Service application that takes requests, performs some work, and provides some output. In cases like that, you can implement a custom mechanism that provides the ability to register and resolve dependencies on a per-request basis if you structure your application properly. The steps you would take are identical to the steps seen in other application types that naturally support per-request semantics.

  * **Build the container at application start.** Make your registrations, build the container, and store a reference to the global container for later use.
  * **When a logical request is received, create a request lifetime scope.** The request lifetime scope should be tagged with the tag ``Autofac.Core.Lifetime.MatchingScopeLifetimeTags.AutofacWebRequest`` so you can use standard registration extension methods like ``InstancePerRequest()``. This will also enable you to share registration modules across application types if you so desire.
  * **Associate request lifetime scope with the request.** This means you need the ability to get the request scope from within the request and not have a single, static, global variable with the "request scope" - that's a threading problem. You either need a construct like ``HttpContext.Current`` (as in ASP.NET) or ``OperationContext.Current`` (as in WCF); or you need to store the request lifetime along with the actual incoming request information (like Web API).
  * **Dispose of the request lifetime after the request is done.** After the request has been processed and the response is sent, you need to call ``IDisposable.Dispose()`` on the request lifetime scope to ensure memory is cleaned up and service instances are released.
  * **Dispose of the container at application end.** When the application is shutting down, call ``IDisposable.Dispose()`` on the global application container to ensure any managed resources are properly disposed and connections to databases, etc. are shut down.

How exactly you do this depends on your application, so an "example" can't really be provided. A good way to see the pattern is to look at the source for :doc:`the integration libraries <../integration/index>` for various app types like MVC and Web API to see how those are done. You can then adopt patterns and adapt accordingly to fit your application's needs.

**This is a very advanced process.** You can pretty easily introduce memory leaks by not properly disposing of things or create threading problems by not correctly associating request lifetimes with requests. Be careful if you go down this road and do a lot of testing and profiling to make sure things work as you expect.
