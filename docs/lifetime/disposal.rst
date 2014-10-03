========
Disposal
========
Resources obtained within a unit of work - database connections, transactions, authenticated sessions, file handles etc. - should be disposed of when that work is complete. .NET provides the ``IDisposable`` interface to aid in this more deterministic notion of disposal.

Some IoC containers need to be told explicitly to dispose of a particular instance, through a method like ``ReleaseInstance()``. This makes it very difficult to guarantee that the correct disposal semantics are used.

* Switching implementations from a non-disposable to a disposable component can mean modifying client code.
* Client code that may have ignored disposal when using shared instances will almost certainly fail to clean up when switched to non-shared instances.

:doc:`Autofac solves these problems using lifetime scopes <index>` as a way of disposing of all of the components created during a unit of work.

.. sourcecode:: csharp

    using (var scope = container.BeginLifetimeScope())
    {
      scope.Resolve<DisposableComponent>().DoSomething();

      // Components for scope disposed here, at the
      // end of the 'using' statement when the scope
      // itself is disposed.
    }

A lifetime scope is created when a unit of work begins, and when that unit of work is complete the nested container can dispose all of the instances within it that are out of scope.

Registering Components
======================

Autofac can automatically dispose of some components, but you have the ability to manually specify a disposal mechanism, too.

Components must be registered as ``InstancePerDependency()`` (the default) or some variation of ``InstancePerLifetimeScope()`` (e.g., ``InstancePerMatchingLifetimeScope()`` or ``InstancePerRequest()``).

If you have singleton components (registered as ``SingleInstance()``) **they will live for the life of the container**. Since container lifetimes are usually the application lifetime, it means the component won't be disposed until the end of the application.

Automatic Disposal
------------------

To take advantage of automatic deterministic disposal, your component must implement ``IDisposable``. You can then register your component as needed and at the end of each lifetime scope in which the component is resolved, the ``Dispose()`` method on the component will be called.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<SomeDisposableComponent>();
    var container = builder.Build();
    // Create nested lifetime scopes, resolve
    // the component, and dispose of the scopes.
    // Your component will be disposed with the scope.

Specified Disposal
------------------

If your component doesn't implement ``IDisposable`` but still requires some cleanup at the end of a lifetime scope, you can use :doc:`the OnRelease lifetime event <events>`.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<SomeComponent>()
           .OnRelease(instance => instance.CleanUp());
    var container = builder.Build();
    // Create nested lifetime scopes, resolve
    // the component, and dispose of the scopes.
    // Your component's "CleanUp()" method will be
    // called when the scope is disposed.

Note that ``OnRelease()`` overrides the default handling of ``IDisposable.Dispose()``. If your component both implements ``IDisposable`` *and* requires some other cleanup method, you will either need to manually call ``Dispose()`` in ``OnRelease()`` or you will need to update your class so the cleanup method gets called from inside ``Dispose()``.

Disabling Disposal
------------------

Components are owned by the container by default and will be disposed by it when appropriate. To disable this, register a component as having external ownership:

.. sourcecode:: csharp

    builder.RegisterType<SomeComponent>().ExternallyOwned();

The container will never call ``Dispose()`` on an object registered with external ownership. It is up to you to dispose of components registered in this fashion.

Another alternative for disabling disposal is to use the :doc:`implicit relationship <../resolve/relationships>` ``Owned<T>`` and :doc:`owned instances <../advanced/owned-instances>`. In this case, rather than putting a dependency ``T`` in your consuming code, you put a dependency on ``Owned<T>``. Your consuming code will then be responsible for disposal.

.. sourcecode:: csharp

    public class Consumer
    {
      private Owned<DisposableComponent> _service;

      public Consumer(Owned<DisposableComponent> service)
      {
        _service = service;
      }

      public void DoWork()
      {
        // _service is used for some task
        _service.Value.DoSomething();

        // Here _service is no longer needed, so
        // it is released
        _service.Dispose();
      }
    }

You can read more about ``Owned<T>`` :doc:`in the owned instances topic <../advanced/owned-instances>`.

Resolve Components from Lifetime Scopes
=======================================

Lifetime scopes are created by calling ``BeginLifetimeScope()``. The simplest way is in a ``using`` block. Use the lifetime scopes to resolve your components and then dispose of the scope when the unit of work is complete.

.. sourcecode:: csharp

    using (var lifetime = container.BeginLifetimeScope())
    {
      var component = lifetime.Resolve<SomeComponent>();
      // component, and any of its disposable dependencies, will
      // be disposed of when the using block completes
    }

Note that with :doc:`Autofac integration libraries <../integration/index>` standard unit-of-work lifetime scopes will be created and disposed for you automatically. For example, in Autofac's :doc:`ASP.NET MVC integration <../integration/mvc>`, a lifetime scope will be created for you at the beginning of a web request and all components will generally be resolved from there. At the end of the web request, the scope will automatically be disposed - no additional scope creation is required on your part. If you are using :doc:`one of the integration libraries <../integration/index>`, you should be aware of what automatically-created scopes are available for you.

You can :doc:`read more about creating lifetime scopes here <working-with-scopes>`.

Child Scopes are NOT Automatically Disposed
===========================================

While lifetime scopes themselves implement ``IDisposable``, the lifetime scopes that you create are **not automatically disposed for you.** If you create a lifetime scope, you are responsible for calling ``Dispose()`` on it to clean it up and trigger the automatic disposal of components. This is done easily with a ``using`` statement, but if you create a scope without a ``using``, don't forget to dispose of it when you're done with it.

It's important to distinguish between scopes **you create** and scopes the **integration libraries create for you**. You don't have to worry about managing integration scopes (like the ASP.NET request scope) - those will be done for you. However, if you manually create your own scope, you will be responsible for cleaning it up.

Advanced Hierarchies
====================

The simplest and most advisable resource management scenario, demonstrated above, is two-tiered: there is a single 'root' container and a lifetime scope is created from this for each unit of work. It is possible to create more complex hierarchies of containers and components, however, using :doc:`tagged lifetime scopes <working-with-scopes>`.