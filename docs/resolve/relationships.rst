===========================
Implicit Relationship Types
===========================

Dependencies using the types below are interpreted specially by Autofac. Where a `relationship type <http://nblumhardt.com/2010/01/the-relationship-zoo/>`_ is listed below, **Autofac will provide an implementation implicitly** wherever needed.

For example, when Autofac is injecting a constructor parameter of type ``IEnumerable<ITask>`` it will **not** look for a component that supplies ``IEnumerable<ITask>``. Instead, the container will find all implementations of ``ITask`` and inject all of them.

(To override this default behaviour it is still possible to register explicit implementations of these types.)

Implicit Relationship Types
---------------------------

=================================================== ==================================================== =======================================================
Relationship                                        Type                                                 Meaning
=================================================== ==================================================== =======================================================
*A* needs *B*                                       ``B``                                                Dependency
*A* needs *B* at some point in the future           ``Lazy<B>``                                          Delayed instantiation
*A* needs *B* until some point in the future        ``Owned<B>``                                         :doc:`Controlled lifetime<../advanced/owned-instances>`
*A* needs to create instances of *B*                ``Func<B>``                                          Dynamic instantiation
*A* provides parameters of types *X* and *Y* to *B* ``Func<X,Y,B>``                                      Parameterisation
*A* needs all the kinds of *B*                      ``IEnumerable<B>``, ``IList<B>``, ``ICollection<B>`` Enumeration
*A* needs to know *X* about *B*                     ``Meta<T>`` and ``Meta<B,X>``                        :doc:`Metadata interrogation<../advanced/metadata>`
*A* needs to choose *B* based on *X*                ``IIndex<X,B>``                                      Lookup
=================================================== ==================================================== =======================================================

Composing Relationship Types
----------------------------

Relationship types can be composed, so:

.. sourcecode:: csharp

    IEnumerable<Func<Owned<ITask>>>

Is interpreted correctly to mean:

 * All implementations, of
 * Factories, that return
 * :doc:`Lifetime-controlled<../advanced/owned-instances>`
 * ``ITask`` services

Standard Composite Relationships
--------------------------------

====================== ========================
.NET Type              Equivalent to
====================== ========================
``Lazy<T,M>``          ``Meta<Lazy<T>, M>``
``ExportFactory<T>``   ``Func<Owned<T>>``
``ExportFactory<T,M>`` ``Meta<Func<Owned<T>>>``
====================== ========================

*Note, the ``ExportFactory`` types are included in Silverlight 4 but not yet .NET. These aren't supported in Autofac yet.*

Relationship Types and Container Independence
---------------------------------------------

The custom relationship types in Autofac don't force you to bind your application more tightly to Autofac. They give you a programming model for container configuration that is consistent with the way you write other components (vs. having to know a lot of specific container extension points and APIs that also potentially centralise your configuration.)

For example, you can still create a custom ``ITaskFactory`` in your core model, but provide an ``AutofacTaskFactory`` implementation based on ``Func<Owned<ITask>>`` if that is desirable.

Example: Implicit IEnumerable Support
-------------------------------------

Let's say you have a dependency interface defined like this:

.. sourcecode:: csharp

    public interface IMessageHandler
    {
      void HandleMessage(Message m);
    }

Further, you have a consumer of dependencies like that where you need to have more than one registered and the consumer needs all of the registered dependencies:

.. sourcecode:: csharp

    public class MessageProcessor
    {
      private IEnumerable<IMessageHandler> _handlers;

      public MessageProcessor(IEnumerable<IMessageHandler> handlers)
      {
        this._handlers = handlers;
      }

      public void ProcessMessage(Message m)
      {
        foreach(var handler in this._handlers)
        {
          handler.HandleMessage(m);
        }
      }
    }

You can easily accomplish this using the implicit ``IEnumerable<T>`` relationship type. Just register all of the dependencies and the consumer, and when you resolve the consumer the *set of all matching dependencies* will be resolved as an enumeration.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<FirstHandler>().As<IMessageHandler>();
    builder.RegisterType<SecondHandler>().As<IMessageHandler>();
    builder.RegisterType<ThirdHandler>().As<IMessageHandler>();
    builder.RegisterType<MessageProcessor>();
    var container = builder.Build();

    using(var scope = container.BeginLifetimeScope())
    {
      // When processor is resolved, it'll have all of the
      // registered handlers passed in to the constructor.
      var processor = scope.Resolve<MessageProcessor>();
      processor.ProcessMessage(m);
    }

Notes on Relationship Types
---------------------------

``IEnumerable<T>`` / ``IList<T>`` / ``ICollection<T>``
------------------------------------------------------

**The enumerable support will return an empty set if no matching items are registered in the container.** That is, using the above example, if you don't register any ``IMessageHandler`` implementations, this will break:

.. sourcecode:: csharp

    // This throws an exception - none are registered!
    container.Resolve<IMessageHandler>();

*However, this works:*

.. sourcecode:: csharp

    // This returns an empty list, NOT an exception:
    container.Resolve<IEnumerable<IMessageHandler>>();

This can create a bit of a "gotcha" where you might think you'll get a null value if you inject something using this relationship. Instead, you'll get an empty list.

``Func<T>`` / ``Func<T, U, V>``
-------------------------------

**Auto-generated function factories cannot have duplicate types in the input parameter list.** For example, say you have a type like this:

.. sourcecode:: csharp

    public class DuplicateTypes
    {
      public DuplicateTypes(int a, int b, string c)
      {
        // ...
      }
    }

You might want to register that type and have an auto-generated function factory for it. *You will be able to resolve the function, but you won't be able to execute it.*

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<DuplicateTypes>();
    var container = builder.Build();
    var func = container.Resolve<Func<int, int, string, DuplicateTypes>>();

    // Throws a DependencyResolutionException:
    var obj = func(1, 2, "three");

In a loosely coupled scenario where the parameters are matched on type, you shouldn't really know about the order of the parameters for a specific object's constructor.

If you need to do something like this, you should use a custom delegate type instead:

.. sourcecode:: csharp

    public delegate DuplicateTypes FactoryDelegate(int a, int b, string c);
    // ...

    var builder = new ContainerBuilder();
    builder.RegisterType<DuplicateTypes>();
    builder.RegisterGeneratedFactory<FactoryDelegate>(new TypedService(typeof(DuplicateTypes)));
    var container = builder.Build();
    var func = container.Resolve<FactoryDelegate>();

    // This will work:
    var obj = func(1, 2, "three");

Should you decide to use the built-in auto-generated factory behavior and only resolve a factory with one of each type, it will work but you'll get the same input for all constructor parameters of the same type.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<DuplicateTypes>();
    var container = builder.Build();
    var func = container.Resolve<Func<int, string, DuplicateTypes>>();

    // This works and is the same as calling
    // new DuplicateTypes(1, 1, "three")
    var obj = func(1, "three");
