=======
Modules
=======

Introduction
============

IoC uses :doc:`components <../glossary>` as the basic building blocks of an application. Providing access to the constructor parameters and properties of components is very commonly used as a means to achieve :doc:`deployment-time configuration<./xml>`.

This is generally a dubious practice for the following reasons:

 * **Constructors can change**: Changes to the constructor signature or properties of a component can break deployed ``App.config`` files - these problems can appear very late in the development process.
 * **XML gets hard to maintain**: Configuration files for large numbers of components can become unwieldy to maintain - this is exacerbated by the fact that XML configuration is weakly-typed and hard to read.
 * **"Code" starts showing up in XML**: Exposing the properties and constructor parameters of classes is an unpleasant breach of the 'encapsulation' of the application's internals - these details don't belong in configuration files.

This is where modules can help.

**A module is a small class that can be used to bundle up a set of related components behind a 'facade' to simplify configuration and deployment.** The module exposes a deliberate, restricted set of configuration parameters that can vary independently of the components used to implement the module.

The components within a module still make use dependencies at the component/service level to access components from other modules.

Advantages of Modules
=====================

Decreased Configuration Complexity
----------------------------------

When configuring an application by IoC it is often necessary to set the parameters spread between multiple components. Modules group related configuration items into one place to reduce the burden of looking up the correct component for a setting.

The implementer of a module determines how the module's configuration parameters map to the properties and constructor parameters of the components inside.

Configuration Parameters are Explicit
-------------------------------------

Configuring an application directly through its components creates a large surface area that will need to be considered when upgrading the application. When it is possible to set potentially any property of any class through a configuration file that will differ at every site, refactoring is no longer safe.

Creating modules limits the configuration parameters that a user can configure, and makes it explicit to the maintenance programmer which parameters these are.

You can also avoid a trade-off between what makes a good program element and what makes a good configuration parameter.

Abstraction from the Internal Application Architecture
------------------------------------------------------

Configuring an application through its components means that the configuration needs to differ depending on things like, for example, the use of an ``enum`` vs. creation of strategy classes. Using modules hides these details of the application's structure, keeping configuration succinct.

Better Type Safety
------------------

A small reduction in type safety will always exist when the classes making up the application can vary based on deployment. Registering large numbers of components through XML configuration, however, exacerbates this problem.

Modules are constructed programmatically, so all of the component registration logic within them can be checked at compile time.

Dynamic Configuration
---------------------

Configuring components within modules is dynamic: the behaviour of a module can vary based on the runtime environment. This is hard, if not impossible, with purely component-based configuration.

Example
=======

In Autofac, modules implement the ``Autofac.Core.IModule`` interface. Generally they will derive from the ``Autofac.Module`` abstract class.

This module provides the ``IVehicle`` service:

.. sourcecode:: csharp

    public class CarTransportModule : Module
    {
      public bool ObeySpeedLimit { get; set; }

      protected override void Load(ContainerBuilder builder)
      {
        builder.Register(c => new Car(c.Resolve<IDriver>())).As<IVehicle>();

        if (ObeySpeedLimit)
          builder.Register(c => new SaneDriver()).As<IDriver>();
        else
          builder.Register(c => new CrazyDriver()).As<IDriver>();
      }
    }

Encapsulated Configuration
--------------------------

Our ``CarTransportModule`` provides the ``ObeySpeedLimit`` configuration parameter without exposing the fact that this is implemented by choosing between a sane or a crazy driver. Clients using the module can use it by declaring their intentions:

.. sourcecode:: csharp

    builder.RegisterModule(new CarTransportModule() {
        ObeySpeedLimit = true
    });

or in XML:

.. sourcecode:: xml

    <module type="CarTransportModule">
      <properties>
        <property name="ObeySpeedLimit" value="true" />
      </properties>
    </module>

This is valuable because the implementation of the module can vary without a flow on effect. That's the idea of encapsulation, after all.

Flexibility to Override
-----------------------

Although clients of the ``CarTransportModule`` are probably primarily concerned with the ``IVehicle`` service, the module registers its ``IDriver`` dependency with the container as well. This ensures that the configuration is still able to be overridden at deployment time in the same way as if the components that make up the module had been registered independently.

It is a 'best practice' when using Autofac to add any XML configuration *after* programmatic configuration, e.g.:

.. sourcecode:: csharp

    builder.RegisterModule(new CarTransportModule());
    builder.RegisterModule(new ConfigurationSettingsReader());

In this way, 'emergency' overrides can be made in the XML configuration file:

.. sourcecode:: xml

    <component
      service="IDriver"
      type="LearnerDriver" />

So, modules increase encapsulation but don't preclude you from tinkering with their innards if you have to.

Adapting to the Deployment Environment
======================================

Modules can be dynamic - that is, they can configure themselves to their execution environment.

When a module is loaded, it can do nifty things like check the environment:

.. sourcecode:: csharp

    protected override void Load(ContainerBuilder builder)
    {
      if (Environment.OSVersion.Platform == PlatformID.Unix)
        RegisterUnixPathFormatter(builder);
      else
        RegisterWindowsPathFormatter(builder);
    }

Common Use Cases for Modules
============================

 * Configure related services that provide a subsystem, e.g. data access with NHibernate
 * Package optional application features as 'plug-ins'
 * Provide pre-built packages for integration with a system, e.g. an accounting system
 * Register a number of similar services that are often used together, e.g. a set of file format converters
 * New or customised mechanisms for configuring the container, e.g. XML configuration is implemented using a module; configuration using attributes could be added this way