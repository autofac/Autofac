=======================
Running Code at Startup
=======================

Autofac provides the ability for components to be notified or automatically activated when the container is built.

There are two automatic activation mechanisms available:
- Startable components
- Auto-activated components

In both cases, **at the time the container is built, the component will be activated**.

Startable Components
====================

A **startable component** is one that is activated by the container when the container is initially built and has a specific method called to bootstrap an action on the component.

The key is to implement the ``Autofac.IStartable`` interface. When the container is built, the component will be activated and the ``IStartable.Start()`` method will be called.

**This only happens once, for a single instance of each component, the first time the container is built.** Resolving startable components by hand won't result in their ``Start()`` method being called. It isn't recommended that startable components implement other services, or be registered as anything other than ``SingleInstance()``.

Components that need to have something like a ``Start()`` method called *each time they are activated* should use :doc:`a lifetime event <events>` like ``OnActivated`` instead.

To create a startable component, implement ``Autofac.IStartable``:

.. sourcecode:: csharp

    public class StartupMessageWriter : IStartable
    {
       public void Start()
       {
          Console.WriteLine("App is starting up!");
       }
    }

Then register your component and **be sure to specify** it as ``IStartable`` or the action won't be called:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder
       .RegisterType<StartupMessageWriter>()
       .As<IStartable>()
       .SingleInstance();

When the container is built, the type will be activated and the ``IStartable.Start()`` method will be called. In this example, a message will be written to the console.

Auto-Activated Components
=========================

An **auto-activated component** is a component that simply needs to be activated one time when the container is built. This is a "warm start" style of behavior where no method on the component is called and no interface needs to be implemented - a single instance of the component will be resolved with no reference to the instance held.

To register an auto-activated component, use the ``AutoActivate()`` registration extension.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder
       .RegisterType<TypeRequiringWarmStart>()
       .AutoActivate();
