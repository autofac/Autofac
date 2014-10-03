===============
Owned Instances
===============

Lifetime and Scope
==================

Autofac controls lifetime using explicitly-delineated scopes. For example, the component providing the ``S`` service, and all of its dependencies, will be disposed/released when the ``using`` block ends:


.. sourcecode:: csharp

    IContainer container = // as per usual
    using (var scope = container.BeginLifetimeScope())
    {
      var s = scope.Resolve<S>();
      s.DoSomething();
    }

*In an IoC container, there’s often a subtle difference between releasing and disposing a component: releasing an owned component goes further than disposing the component itself. Any of the dependencies of the component will also be disposed. Releasing a shared component is usually a no-op, as other components will continue to use its services.*

Relationship Types
==================

Autofac has a system of :doc:`relationship types <../resolve/relationships>` that can be used to provide the features of the container in a declarative way. Instead of manipulating an ``IContainer`` or ``ILifetimeScope`` directly, as in the above example, relationship types allow a component to specify exactly which container services are needed, in a minimal, declarative way.

Owned instances are consumed using the ``Owned<T>`` relationship type.

Owned of T
----------

An owned dependency can be released by the owner when it is no longer required. Owned dependencies usually correspond to some unit of work performed by the dependent component.

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

When ``Consumer`` is created by the container, the ``Owned<DisposableComponent>`` that it depends upon will be created inside its own lifetime scope. When ``Consumer`` is finished using the ``DisposableComponent``, disposing the ``Owned<DisposableComponent>`` reference will end the lifetime scope that contains ``DisposableComponent``. This means that all of ``DisposableComponent``'s non-shared, disposable dependencies will also be released.

Combining Owned with Func
-------------------------

Owned instances are usually used in conjunction with a ``Func<T>`` relationship, so that units of work can be begun and ended on-the-fly.

.. sourcecode:: csharp

    interface IMessageHandler
    {
      void Handle(Message message);
    }

    class MessagePump
    {
      Func<Owned<IMessageHandler>> _handlerFactory;

      public MessagePump(Func<Owned<IMessageHandler>> handlerFactory)
      {
        _handlerFactory = handlerFactory;
      }

      public void Go()
      {
        while(true)
        {
          var message = NextMessage();

          using (var handler = _handlerFactory())
          {
            handler.Value.Handle(message);
          }
        }
      }
    }


Owned and Tags
--------------

The lifetimes created by ``Owned<T>`` use the tagging feature present as ``ILifetimeScope.Tag``. The tag applied to a lifetime of ``Owned<T>`` will be ``new TypedService(typeof(T))`` - that is, the tag of the lifetime reflects its entry point.