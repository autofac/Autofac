========================
Named and Keyed Services
========================

[TODO: Cross reference the :doc:`metadata section on WithKeyAttribute <metadata>`.]

Autofac provides three typical ways to identify services. The most common is to identify by type:

.. sourcecode:: csharp

    builder.Register<OnlineState>().As<IDeviceState>();

This example associates the ``IDeviceState`` typed service with the ``OnlineState`` component. Instances of the component can be retrieved using the service type with the ``Resolve()`` method:

.. sourcecode:: csharp

    var r = container.Resolve<IDeviceState>();

However, you can also identify services by a string name or by an object key.

Named Services
==============

Services can be further identified using a service name. Using this technique, the ``Named()`` registration method replaces ``As()``.

.. sourcecode:: csharp

    builder.Register<OnlineState>().Named<IDeviceState>("online");

To retrieve a named service, the ``ResolveNamed()`` method is used:

.. sourcecode:: csharp

    var r = container.ResolveNamed<IDeviceState>("online");

**Named services are simply keyed services that use a string as a key**, so the techniques described in the next section apply equally to named services.

Keyed Services
==============

Using strings as component names is convenient in some cases, but in others we may wish to use keys of other types. Keyed services provide this ability.

For example, an enum may describe the different device states in our example:

.. sourcecode:: csharp

    public enum DeviceState { Online, Offline }

Each enum value corresponds to an implementation of the service:

.. sourcecode:: csharp

    public class OnlineState : IDeviceState { }

The enum values can then be registered as keys for the implementations as shown below.

.. sourcecode:: csharp

     var builder = new ContainerBuilder();
    builder.RegisterType<OnlineState>().Keyed<IDeviceState>(DeviceState.Online);
    builder.RegisterType<OfflineState>().Keyed<IDeviceState>(DeviceState.Offline);
    // Register other components here

Resolving Explicitly
--------------------

The implementation can be resolved explicitly with ``ResolveKeyed()``:

.. sourcecode:: csharp

    var r = container.ResolveKeyed<IDeviceState>(DeviceState.Online);

This does however result in using the container as a Service Locator, which is discouraged. As an alternative to this pattern, the ``IIndex`` type is provided.

Resolving with an Index
-----------------------

``Autofac.Features.Indexed.IIndex<K,V>`` is a :doc:`relationship type that Autofac implements automatically <../resolve/relationships>`. Components that need to choose between service implementations based on a key can do so by taking a constructor parameter of type ``IIndex<K,V>``.

.. sourcecode:: csharp

    public class Modem : IHardwareDevice
    {
      IIndex<DeviceState, IDeviceState> _states;
      IDeviceState _currentState;

      public Modem(IIndex<DeviceState, IDeviceState> states)
      {
         _states = states;
         SwitchOn();
      }

      void SwitchOn()
      {
         _currentState = _states[DeviceState.Online];
      }
    }


In the ``SwitchOn()`` method, the index is used to find the implementation of ``IDeviceState`` that was registered with the ``DeviceState.Online`` key.