====================
Handling Concurrency
====================

Autofac is designed for use in highly-concurrent applications. The guidance below will help you be successful in these situations.

Component Registration
----------------------

``ContainerBuilder`` **is not thread-safe** and is designed to be used only on a single thread at the time the application starts up. This is the most common scenario and works for almost all applications.

**Registration into a container** *after* **is is built, using** ``ContainerBuilder.Update()``, **also is not thread-safe.** For applications that register components after the container has been built (which should be very uncommon) additional locking to protect the container from concurrent access during an ``Update()`` operation is necessary.

Service Resolution
------------------

**All container operations are safe for use between multiple threads.**

To reduce locking overhead, each ``Resolve`` operation takes place in a 'context' that provides the dependency-resolution features of the container. This is the parameter provided to component registration delegates.

**Resolution context objects are single-threaded** and should **not** be used except during the course of a dependency resolution operation.

Avoid component registrations that store the context:

.. sourcecode:: csharp

    // THIS IS BROKEN - DON'T DO IT
    builder.Register(c => new MyComponent(c));

In the above example, the "c" ``IComponentContext`` parameter is being provided to MyComponent (which takes ``IComponent`` as a dependency).  This code is incorrect because the temporary "c" parameter will be reused.

Instead resolve ``IComponentContext`` from "c" to access the non-temporary context:

.. sourcecode:: csharp

    builder.Register(c =>
    {
      IContext threadSpecificContext = c.Resolve<IComponentContext>(); // access real context.
      return new MyComponent(threadSpecificContext);
    }

Take care also not to initialize components with closures over the "c" parameter, as any reuse of "c" will cause issues.

The container hierarchy mechanism further reduces locking, by maintaining local copies of the component registrations for any factory/container components. Once the initial registration copy has been made, a thread using an 'inner' container can create or access such components without blocking any other thread.

Lifetime Events
---------------

When making use of the LifetimeEvents available, don't call back into the container in handlers for the ``Preparing``, ``Activating`` or ``Activated`` events: use the supplied ``IComponentContext`` instead.

Thread Scoped Services
----------------------

You can use Autofac to register services that are specific to a thread. The ThreadScoping page has more information on this.

Internals
---------

Keeping in mind the guidelines above, here's a little more specific information about thread safety and locking in Autofac.

Thread-Safe Types
-----------------

The following types are safe for concurrent access by multiple threads:

 * ``Container``
 * ``ComponentRegistry``
 * ``Disposer`` (default implementation of ``IDisposer``)
 * ``LifetimeScope`` (default implementation of ``ILifetimeScope``)

These types cover practically all of the runtime/resolution scenarios.

The following types are designed for single-threaded access at configuration time:

 * ``ContainerBuilder``

So, a correct Autofac application will use a ``ContainerBuilder`` on a single thread to create the container at startup. Subsequent use of the container can occur on any thread.

Deadlock Avoidance
------------------

Autofac is designed in such a way that deadlocks won't occur in normal use. This section is a guide for maintainers or extension writers.

Locks may be acquired in the following order:

 * A thread holding a lock for any of the following may not acquire any further locks:

   * ``ComponentRegistry``
   * ``Disposer``

 * A thread holding the lock for a ``LifetimeScope`` may subsequently acquire the lock for:

   * Its parent ``LifetimeScope``
   * Any of the items listed above