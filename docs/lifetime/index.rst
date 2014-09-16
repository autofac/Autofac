==============================
Controlling Scope and Lifetime
==============================

A great place to start learning about Autofac scope and lifetime is in `Nick Blumhardt's Autofac lifetime primer <http://nblumhardt.com/2011/01/an-autofac-lifetime-primer/>`_. There's a lot to digest, though, and a lot of intermixed concepts there, so we'll try to complement that article here.

You may recall from the :doc:`registration topic <../register/registration>` that you add **components** to the container that implement **services**. You then end up :doc:`resolving services <..\resolve\index>` and using those service instances to do your work.

The **lifetime** of a service is how long the service instance will live in your application - from the original instantiation to :doc:`disposal <disposal>`. For example, if you "new up" an object that implements `IDisposable <http://msdn.microsoft.com/en-us/library/system.idisposable.aspx>`_ and then later call ``Dispose()`` on it, the lifetime of that object is from the time you instantiated it all the way through disposal (or garbage collection if you didn't proactively dispose it).

The **scope** of a service is the area in the application where that service can be shared with other components that consume it. For example, in your application you may have a global static singleton - the "scope" of that global object instance would be the whole application. On the other hand, you might create a local variable in a ``for`` loop that makes use of the global singleton - the local variable has a much smaller scope than the global.

The concept of a **lifetime scope** in Autofac combines these two notions. Effectively, a lifetime scope equates with a unit of work in your application. A unit of work might begin a lifetime scope at the start, then services required for that unit of work get resolved from a lifetime scope. As you resolve services, Autofac tracks disposable (``IDisposable``) components that are resolved. At the end of the unit of work, you dispose of the associated lifetime scope and Autofac will automatically clean up/dispose of the resolved services.

Lifetime scopes are nestable and they control how components are shared. For example, a "singleton" service might be resolved from a root lifetime scope while individual units of work may require their own instances of other services.

The easiest way to look at this is from a real-life standpoint. Let's use a web application as an example. Say you have the following scenario:

- You have a global singleton logging service.
- Two simultaneous requests come in to the web application.
- Each request is a logical "unit of work" and each requires its own order processing service.
- Each order processing service needs to log information to the logging service.

In this scenario, you'd have a root lifetime scope that contains the singleton logging service and you'd have one child lifetime scope per request, each with its own order processing service.

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



.. toctree::

    instance-scope.rst
    disposal.rst
    events.rst
    startup.rst