==============================
Controlling Scope and Lifetime
==============================

A great place to start learning about Autofac scope and lifetime is in `Nick Blumhardt's Autofac lifetime primer <http://nblumhardt.com/2011/01/an-autofac-lifetime-primer/>`_. There's a lot to digest, though, and a lot of intermixed concepts there, so we'll try to complement that article here.

You may recall from the :doc:`registration topic <../register/registration>` that you add **components** to the container that implement **services**. You then end up :doc:`resolving services <../resolve/index>` and using those service instances to do your work.

The **lifetime** of a service is how long the service instance will live in your application - from the original instantiation to :doc:`disposal <disposal>`. For example, if you "new up" an object that implements `IDisposable <http://msdn.microsoft.com/en-us/library/system.idisposable.aspx>`_ and then later call ``Dispose()`` on it, the lifetime of that object is from the time you instantiated it all the way through disposal (or garbage collection if you didn't proactively dispose it).

The **scope** of a service is the area in the application where that service can be shared with other components that consume it. For example, in your application you may have a global static singleton - the "scope" of that global object instance would be the whole application. On the other hand, you might create a local variable in a ``for`` loop that makes use of the global singleton - the local variable has a much smaller scope than the global.

The concept of a **lifetime scope** in Autofac combines these two notions. Effectively, a lifetime scope equates with a unit of work in your application. A unit of work might begin a lifetime scope at the start, then services required for that unit of work get resolved from a lifetime scope. As you resolve services, Autofac tracks disposable (``IDisposable``) components that are resolved. At the end of the unit of work, you dispose of the associated lifetime scope and Autofac will automatically clean up/dispose of the resolved services.

**The two important things lifetime scopes control are sharing and disposal.**

- **Lifetime scopes are nestable and they control how components are shared.** For example, a "singleton" service might be resolved from a root lifetime scope while individual units of work may require their own instances of other services. You can determine how a component is shared by :doc:`setting its instance scope at registration <instance-scope>`.
- **Lifetime scopes track disposable objects and dispose of them when the lifetime scope is disposed.** For example, if you have a component that implements ``IDisposable`` and you resolve it from a lifetime scope, the scope will hold onto it and dispose of it for you so your service consumers don't have to know about the underlying implementation. :doc:`You have the ability to control this behavior or add new disposal behavior if you choose. <disposal>`

As you work in your application, it's good to remember these concepts so you make the most efficient use of your resources.

    **It is important to always resolve services from a lifetime scope and not the root container.** Due to the disposal tracking nature of lifetime scopes, if you resolve a lot of disposable components from the container (the "root lifetime scope"), you may inadvertently cause yourself a memory leak. The root container will hold references to those disposable components for as long as it lives (usually the lifetime of the application) so it can dispose of them. :doc:`You can change disposal behavior if you choose <disposal>`, but it's a good practice to only resolve from a scope. If Autofac detects usage of a singleton or shared component, it will automatically place it in the appropriate tracking scope.

Let's look at a web application as a more concrete example to illustrate lifetime scope usage. Say you have the following scenario:

- You have a global singleton logging service.
- Two simultaneous requests come in to the web application.
- Each request is a logical "unit of work" and each requires its own order processing service.
- Each order processing service needs to log information to the logging service.

In this scenario, you'd have a root lifetime scope that contains the singleton logging service and you'd have one child lifetime scope per request, each with its own order processing service::

    +---------------------------------------------------+
    |                 Autofac Container                 |
    |                Root Lifetime Scope                |
    |                                                   |
    |                  Logging Service                  |
    |            (shared across all requests)           |
    |                                                   |
    | +----------------------+ +----------------------+ |
    | |  First Request Scope | | Second Request Scope | |
    | |                      | |                      | |
    | |   Order Processor    | |   Order Processor    | |
    | +----------------------+ +----------------------+ |
    +---------------------------------------------------+

When each request ends, the request lifetime scope ends and the respective order processor gets disposed. The logging service, as a singleton, stays alive for sharing by future requests.

You can dive deeper on lifetime scopes in `Nick Blumhardt's Autofac lifetime primer <http://nblumhardt.com/2011/01/an-autofac-lifetime-primer/>`_.

**Additional lifetime scope topics to explore:**

.. toctree::

    instance-scope.rst
    disposal.rst
    events.rst
    startup.rst