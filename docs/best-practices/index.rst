==================================
Best Practices and Recommendations
==================================

You can always ask for Autofac usage guidance `on StackOverflow <http://stackoverflow.com/questions/tagged/autofac>`_ using the ``autofac`` tag or in `the discussion group <https://groups.google.com/forum/#forum/autofac>`_, but these quick tips can help get you going.

Always Resolve Dependencies from Nested Lifetimes
=================================================

Autofac is designed to :doc:`track and dispose of resources <../lifetime/disposal>` for you. To ensure this happens, make sure that long-running applications are partitioned into units of work (requests or transactions) and that services are resolved through unit of work level lifetime scopes. The :doc:`per-request lifetime scope support in ASP.NET <../faq/per-request-scope>` is an example of this technique.

Structure Configuration with Modules
=====================================

:doc:`Autofac modules <../configuration/modules>` give structure to container configuration and allow deployment-time settings to be injected. Rather than using :doc:`XML configuration <../configuration/xml>` alone, consider modules for a more flexible approach. Modules can always be combined with XML configuration for a best-of-both-worlds experience.

Use As<T>() in Delegate Registrations
=====================================

Autofac infers implementation type from the expressions you use to register components:

.. sourcecode:: csharp

    builder.Register(c => new Component()).As<IComponent>();

...makes the type ``Component`` the ``LimitType`` of the component. These other type casting mechanisms are equivalent but don't provide the correct ``LimitType``:

.. sourcecode:: csharp

    // Works, but avoid this
    builder.Register(c => (IComponent)new Component());

    // Works, but avoid this
    builder.Register<IComponent>(c => new Component());

Use Constructor Injection
=========================

The concept of using constructor injection for required dependencies and property injection for optional dependencies is quite well known. An alternative, however, is to use the `"Null Object" <http://en.wikipedia.org/wiki/Null_Object_pattern>`_ or `"Special Case" <http://martinfowler.com/eaaCatalog/specialCase.html>`_ pattern to provide default, do-nothing implementations for the optional service. This prevents the possibility of special-case code in the implementation of the component (e.g. ``if (Logger != null) Logger.Log("message");``).

Use Relationship Types, Not Service Locators
============================================

Giving components access to the container, storing it in a public static property, or making functions like ``Resolve()`` available on a global "IoC" class defeats the purpose of using dependency injection. Such designs have more in common with the `Service Locator <http://martinfowler.com/articles/injection.html#UsingAServiceLocator>`_ pattern.

If components have a dependency on the container (or on a lifetime scope), look at how they're using the container to retrieve services, and add those services to the component's (dependency injected) constructor arguments instead.

Use :doc:`relationship types <../resolve/relationships>` for components that need to instantiate other components or interact with the container in more advanced ways.

Register Components from Least-to-Most Specific
===============================================

Autofac overrides component registrations by default. This means that an application can register all of its default components, then read an associated configuration file to override any that have been customized for the deployment environment.

Use Profilers for Performance Checking

Before doing any performance optimization or making assumptions about potential memory leaks, **always run a profiler** like `SlimTune <http://code.google.com/p/slimtune/>`_, `dotTrace <http://www.jetbrains.com/profiler/>`_, or `ANTS <http://www.red-gate.com/products/dotnet-development/ants-performance-profiler/>`_ to see where time is truly being spent. It might not be where you think.

Register Once, Resolve Many
===========================

Don't register components during units of work if you can avoid it; it is more expensive to register a component than resolve one. Use nested lifetime scopes and appropriate :doc:`instance scopes <../lifetime/instance-scope>` to keep per-unit-of-work instances separate.

Register Frequently-Used Components with Lambdas
================================================

If you do need to squeeze extra performance out of Autofac, your best bet is to identify the most frequently-created components and register them using an expression rather than by type, e.g.:

.. sourcecode:: csharp

    builder.RegisterType<Component>();

Becomes:

.. sourcecode:: csharp

    builder.Register(c => new Component());

This can yield an improvement of up to 10x faster ``Resolve()`` calls, but only makes sense for components that appear in many object graphs. See :doc:`the registration documentation <../register/index>` for more on lambda components.