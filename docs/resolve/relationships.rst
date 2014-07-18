===========================
Implicit Relationship Types
===========================

Autofac supports automatically resolving particular types implicitly to support special relationships between :doc:`components and services <../glossary>`. To take advantage of these relationships, simply register your components as normal, but change the constructor parameter in the consuming component or the type being resolved in the ``Resolve()`` call so it takes in the specified relationship type.

For example, when Autofac is injecting a constructor parameter of type ``IEnumerable<ITask>`` it will **not** look for a component that supplies ``IEnumerable<ITask>``. Instead, the container will find all implementations of ``ITask`` and inject all of them.

(Don't worry - there are examples below showing the usage of the various types and what they mean.)

Note: To override this default behavior *it is still possible to register explicit implementations of these types*.

[Content on this document based on Nick Blumhardt's blog article `The Relationship Zoo <http://nblumhardt.com/2010/01/the-relationship-zoo/>`_.]


Supported Relationship Types
============================

The table below summarizes each of the supported relationship types in Autofac and shows the .NET type you can use to consume them. Each relationship type has a more detailed description and use case after that.

=================================================== ==================================================== =======================================================
Relationship                                        Type                                                 Meaning
=================================================== ==================================================== =======================================================
*A* needs *B*                                       ``B``                                                Direct Dependency
*A* needs *B* at some point in the future           ``Lazy<B>``                                          Delayed Instantiation
*A* needs *B* until some point in the future        ``Owned<B>``                                         :doc:`Controlled Lifetime <../advanced/owned-instances>`
*A* needs to create instances of *B*                ``Func<B>``                                          Dynamic Instantiation
*A* provides parameters of types *X* and *Y* to *B* ``Func<X,Y,B>``                                      Parameterized Instantiation
*A* needs all the kinds of *B*                      ``IEnumerable<B>``, ``IList<B>``, ``ICollection<B>`` Enumeration
*A* needs to know *X* about *B*                     ``Meta<B>`` and ``Meta<B,X>``                        :doc:`Metadata Interrogation <../advanced/metadata>`
*A* needs to choose *B* based on *X*                ``IIndex<X,B>``                                      :doc:`Keyed Service <../advanced/keyed-services>` Lookup
=================================================== ==================================================== =======================================================

.. contents:: Relationship Type Details
  :local:
  :depth: 1


Direct Dependency (B)
---------------------
A *direct dependency* relationship is the most basic relationship supported - component ``A`` needs service ``B``. This is handled automatically through standard constructor and property injection:

.. sourcecode:: csharp

    public class A
    {
      public A(B dependency) { ... }
    }

Register the ``A`` and ``B`` components, then resolve:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<A>();
    builder.RegisterType<B>();
    var container = builder.Build();

    using(var scope = container.BeginLifetimeScope())
    {
      // B is automatically injected into A.
      var a = scope.Resolve<A>();
    }


Delayed Instantiation (Lazy<B>)
-------------------------------
A *lazy dependency* is not instantiated until its first use. This appears where the dependency is infrequently used, or expensive to construct. To take advantage of this, use a ``Lazy<B>`` in the constructor of ``A``:

.. sourcecode:: csharp

    public class A
    {
      Lazy<B> _b;

      public A(Lazy<B> b) { _b = b }

      public void M()
      {
          // The component implementing B is created the
          // first time M() is called
          _b.Value.DoSomething();
      }
    }

If you have a lazy dependency for which you also need metadata, you can use ``Lazy<B,M>`` instead of the longer ``Meta<Lazy<B>, M>``.


Controlled Lifetime (Owned<B>)
------------------------------
An *owned dependency* can be released by the owner when it is no longer required. Owned dependencies usually correspond to some unit of work performed by the dependent component.

This type of relationship is interesting particularly when working with components that implement ``IDisposable``. :doc:`Autofac automatically disposes of disposable components <../lifetime/disposal>` at the end of a lifetime scope, but that may mean a component is held onto for too long; or you may just want to take control of disposing the object yourself. In this case, you'd use an *owned dependency*.

.. sourcecode:: csharp

    public class A
    {
      Owned<B> _b;

      public A(Owned<B> b) { _b = b; }

      public void M()
      {
          // _b is used for some task
          _b.Value.DoSomething();

          // Here _b is no longer needed, so
          // it is released
          _b.Dispose();
      }
    }

Internally, Autofac creates a tiny lifetime scope in which the ``B`` service is resolved, and when you call ``Dispose()`` on it, the lifetime scope is disposed. What that means is that disposing of ``B`` will *also dispose of its dependencies* unless those dependencies are shared (e.g., singletons).

This also means that if you have ``InstancePerLifetimeScope()`` registrations and you resolve one as ``Owned<B>`` then you may not get the same instance as being used elsewhere in the same lifetime scope. This example shows the gotcha:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<A>().InstancePerLifetimeScope();
    builder.RegisterType<B>().InstancePerLifetimeScope();
    var container = builder.Build();

    using(var scope = container.BeginLifetimeScope())
    {
      // Here we resolve a B that is InstancePerLifetimeScope();
      var b1 = scope.Resolve<B>();
      b1.DoSomething();

      // This will be the same as b1 from above.
      var b2 = scope.Resolve<B>();
      b2.DoSomething();

      // The B used in A will NOT be the same as the others.
      var a = scope.Resolve<A>();
      a.M();
    }

This is by design because you wouldn't want one component to dispose the ``B`` out from under everything else. However, it may lead to some confusion if you're not aware.

If you would rather control ``B`` disposal yourself all the time, :doc:`register B as ExternallyOwned() <../lifetime/disposal>`.


Dynamic Instantiation (Func<B>)
-------------------------------
Using an *auto-generated factory* can let you effectively call ``Resolve<T>()`` without tying your component to Autofac. Use this relationship type if you need to create more than one instance of a given service, or if you're not sure if you're going to need a service and want to make the decision at runtime. This relationship is also useful in cases like :doc:`WCF integration <../integration/wcf>` where you need to create a new service proxy after faulting the channel.

An example of this relationship looks like:

.. sourcecode:: csharp

    public class A
    {
      Func<B> _b;

      public A(Func<B> b) { _b = b; }

      public void M()
      {
          var b = _b();
          b.DoSomething();
      }
    }


Parameterized Instantiation (Func<X, Y, B>)
-------------------------------------------
You can also use an *auto-generated factory* to pass strongly-typed parameters to the resolution function. This is an alternative to :doc:`passing parameters during registration <../register/parameters>` or :doc:`passing during manual resoution <../resolve/parameters>`:

.. sourcecode:: csharp

    public class A
    {
        Func<int, string, B> _b;

        public A(Func<int, string, B> b) { _b = b }

        public void M()
        {
            var b = _b(42, "http://hel.owr.ld");
            b.DoSomething();
        }
    }

Internally, Autofac treats these as typed parameters. What that means is that **auto-generated function factories cannot have duplicate types in the input parameter list.** For example, say you have a type like this:

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

    var func = scope.Resolve<Func<int, int, string, DuplicateTypes>>();

    // Throws a DependencyResolutionException:
    var obj = func(1, 2, "three");

In a loosely coupled scenario where the parameters are matched on type, you shouldn't really know about the order of the parameters for a specific object's constructor. If you need to do something like this, you should use a custom delegate type instead:

.. sourcecode:: csharp

    public delegate DuplicateTypes FactoryDelegate(int a, int b, string c);

Then register that delegate using ``RegisterGeneratedFactory()``:

.. sourcecode:: csharp

    builder.RegisterType<DuplicateTypes>();
    builder.RegisterGeneratedFactory<FactoryDelegate>(new TypedService(typeof(DuplicateTypes)));

Now the function will work:

.. sourcecode:: csharp

    var func = scope.Resolve<FactoryDelegate>();
    var obj = func(1, 2, "three");

Another option you have is to use a :doc:`delegate factory, which you can read about in the advanced topics section <../advanced/delegate-factories>`.

Should you decide to use the built-in auto-generated factory behavior (``Func<X, Y, B>``) and only resolve a factory with one of each type, it will work but you'll get the same input for all constructor parameters of the same type.

.. sourcecode:: csharp

    var func = container.Resolve<Func<int, string, DuplicateTypes>>();

    // This works and is the same as calling
    // new DuplicateTypes(1, 1, "three")
    var obj = func(1, "three");

You can read more about delegate factories and the ``RegisterGeneratedFactory()`` method :doc:`in the advanced topics section <../advanced/delegate-factories>`.


Enumeration (IEnumerable<B>, IList<B>, ICollection<B>)
------------------------------------------------------
Dependencies of an *enumerable type* provide multiple implementations of the same service (interface). This is helpful in cases like message handlers, where a message comes in and more than one handler is registered to process the message.

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

You can easily accomplish this using the implicit enumerable relationship type. Just register all of the dependencies and the consumer, and when you resolve the consumer the *set of all matching dependencies* will be resolved as an enumeration.

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

**The enumerable support will return an empty set if no matching items are registered in the container.** That is, using the above example, if you don't register any ``IMessageHandler`` implementations, this will break:

.. sourcecode:: csharp

    // This throws an exception - none are registered!
    scope.Resolve<IMessageHandler>();

*However, this works:*

.. sourcecode:: csharp

    // This returns an empty list, NOT an exception:
    scope.Resolve<IEnumerable<IMessageHandler>>();

This can create a bit of a "gotcha" where you might think you'll get a null value if you inject something using this relationship. Instead, you'll get an empty list.


Metadata Interrogation (Meta<B>, Meta<B, X>)
--------------------------------------------
The :doc:`Autofac metadata feature <../advanced/metadata>` lets you associate arbitrary data with services that you can use to make decisions when resolving. If you want to make those decisions in the consuming component, use the ``Meta<B>`` relationship, which will provide you with a string/object dictionary of all the object metadata:

.. sourcecode:: csharp

    public class A
    {
      Meta<B> _b;

      public A(Meta<B> b) { _b = b; }

      public void M()
      {
        if (_b.Metadata["SomeValue"] == "yes")
        {
          _b.Value.DoSomething();
        }
      }
    }

You can use :doc:`strongly-typed metadata <../advanced/metadata>` as well, by specifying the metadata type in the ``Meta<B, X>`` relationship:

.. sourcecode:: csharp

    public class A
    {
      Meta<B, BMetadata> _b;

      public A(Meta<B, BMetadata> b) { _b = b; }

      public void M()
      {
        if (_b.Metadata.SomeValue == "yes")
        {
          _b.Value.DoSomething();
        }
      }
    }

If you have a lazy dependency for which you also need metadata, you can use ``Lazy<B,M>`` instead of the longer ``Meta<Lazy<B>, M>``.

Keyed Service Lookup (IIndex<X, B>)
-----------------------------------
In the case where you have many of a particular item (like the ``IEnumerable<B>`` relationship) but you want to pick one based on :doc:`service key <../advanced/keyed-services>`, you can use the ``IIndex<X, B>`` relationship. First, register your services with keys:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<DerivedB>().Keyed<B>("first");
    builder.RegisterType<AnotherDerivedB>().Keyed<B>("second");
    builder.RegisterType<A>();
    var container = builder.Build();

Then consume the ``IIndex<X, B>`` to get a dictionary of keyed services:

.. sourcecode:: csharp

    public class A
    {
      IIndex<string, B> _b;

      public A(IIndex<string, B> b) { _b = b; }

      public void M()
      {
        var b = this._b["first"];
        _b.DoSomething();
      }
    }


Composing Relationship Types
============================

Relationship types can be composed, so:

.. sourcecode:: csharp

    IEnumerable<Func<Owned<ITask>>>

Is interpreted correctly to mean:

 * All implementations, of
 * Factories, that return
 * :doc:`Lifetime-controlled<../advanced/owned-instances>`
 * ``ITask`` services

Relationship Types and Container Independence
=============================================
The custom relationship types in Autofac based on standard .NET types don't force you to bind your application more tightly to Autofac. They give you a programming model for container configuration that is consistent with the way you write other components (vs. having to know a lot of specific container extension points and APIs that also potentially centralize your configuration).

For example, you can still create a custom ``ITaskFactory`` in your core model, but provide an ``AutofacTaskFactory`` implementation based on ``Func<Owned<ITask>>`` if that is desirable.

Note that some relationships are based on types that are in Autofac (e.g., ``IIndex<X, B>``). Using those relationship types do tie you to at least having a reference to Autofac, even if you choose to use a different IoC container for the actual resolution of services.