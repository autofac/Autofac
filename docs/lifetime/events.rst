===============
Lifetime Events
===============

Autofac exposes events that can be hooked at various stages in instance lifecycle. These are subscribed to during component registration (or alternatively by attaching to the ``IComponentRegistration`` interface.

.. contents::
  :local:

OnActivating
============

The ``OnActivating`` event is raised before a component is used. Here you can:

* Switch the instance for another or wrap it in a proxy
* :doc:`Do property injection or method injection <../register/prop-method-injection>`
* Perform other initialization tasks

In some cases, such as with ``RegisterType<T>()``, the concrete type registered is used for type resolution and used by ``ActivatingEventArgs``. For example, the following will fail with a class cast exception:

.. sourcecode:: csharp

    builder.RegisterType<TConcrete>() // FAILS: will throw at cast of TInterfaceSubclass
           .As<TInterface>()          // to type TConcrete
           .OnActivating(e => e.ReplaceInstance(new TInterfaceSubclass()));

A simple workaround is to do the registration in two steps:

.. sourcecode:: csharp

    builder.RegisterType<TConcrete>().AsSelf();
    builder.Register<TInterface>(c => c.Resolve<TConcrete>())
           .OnActivating(e => e.ReplaceInstance(new TInterfaceSubclass()));

OnActivated
===========

The ``OnActivated`` event is raised once a component is fully constructed. Here you can perform application-level tasks that depend on the component being fully constructed - *these should be rare*.

OnRelease
=========

The ``OnRelease`` event replaces :doc:`the standard cleanup behavior for a component <disposal>`. The standard cleanup behavior of components that implement ``IDisposable`` and that are not marked as ``ExternallyOwned()`` is to call the ``Dispose()`` method. The standard cleanup behavior for components that do not implement ``IDisposable`` or are marked as externally owned is a no-op - to do nothing. ``OnRelease`` overrides this behavior with the provided implementation.