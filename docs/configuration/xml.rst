=================
XML Configuration
=================

Most IoC containers provide a programmatic interface as well as XML configuration support, and Autofac is no exception.

Autofac encourages programmatic configuration through the ``ContainerBuilder`` class. Using the programmatic interface is central to the design of the container. XML is recommended when concrete classes cannot be chosen or configured at compile-time.

Before diving too deeply into XML configuration, be sure to read :doc:`Modules<./modules>` - this explains how to handle more complex scenarios than the basic XML component registration will allow.

Syntax
======

Autofac can read standard .NET application config files. These are the ones called ``app.config``.

You need to declare a section handler somewhere near the top of your config file::

    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
        <configSections>
            <section name="autofac" type="Autofac.Configuration.SectionHandler, Autofac.Configuration"/>
        </configSections>

Then, provide a section describing your components::

    <autofac defaultAssembly="Autofac.Example.Calculator.Api">
        <components>
            <component
                type="Autofac.Example.Calculator.Addition.Add, Autofac.Example.Calculator.Addition"
                service="Autofac.Example.Calculator.Api.IOperation" />

            <component
                type="Autofac.Example.Calculator.Division.Divide, Autofac.Example.Calculator.Division"
                service="Autofac.Example.Calculator.Api.IOperation" >
                <parameters>
                    <parameter name="places" value="4" />
                </parameters>
            </component>

The ``defaultAssembly`` attribute is optional, allowing namespace-qualified rather than fully-qualified type names to be used. This can save some clutter and typing, especially if you use one configuration file per assembly (see Additional Config Files below.)

Valid 'component' Attributes
============================

The following can be used as attributes on the ``component`` element (defaults are the same as for the programmatic API):

====================== =============================================================================================================================== =================================================================
Attribute Name         Description                                                                                                                     Valid Values
====================== =============================================================================================================================== =================================================================
``type``               The only required attribute. The concrete class of the component (assembly-qualified if in an assembly other than the default.) Any .NET type name that can be created through reflection.
``service``            A service exposed by the component. For more than one service, use the nested ``services`` element.                             As for ``type``.
``instance-scope``     Instance scope - see :doc:`Instance Scope<../lifetime/instance-scope>`.                                                         ``per-dependency``, ``single-instance`` or ``per-lifetime-scope``
``instance-ownership`` Container's ownership over the instances - see the ``InstanceOwnership`` enumeration.                                           ``lifetime-scope`` or ``external``
``name``               A string name for the component.                                                                                                Any non-empty string value.
``inject-properties``  Enable property (setter) injection for the component.                                                                           ``yes``, ``no``.
====================== =============================================================================================================================== =================================================================

Valid 'component' Nested Elements
=================================

============== =======================================================================================================================================================
Element        Description
============== =======================================================================================================================================================
``services``   A list of ``service`` elements, whose element content contains the names of types exposed as services by the component (see the ``service`` attribute.)
``parameters`` A list of explicit constructor parameters to set on the instances (see example above.)
``properties`` A list of explicit property values to set (syntax as for ``parameters``.)
``metadata``   A list of ``item`` nodes with ``name``, ``value`` and ``type`` attributes.
============== =======================================================================================================================================================

There are some features missing from the XML configuration syntax that are available through the programmatic API - for example registration of generics. Using modules is recommended in these cases.

Modules
=======

Configuring the container using components is very fine-grained and can get verbose quickly. Autofac has support for packaging components into :doc:`Modules<./modules>` in order to encapsulate implementation while providing flexible configuration. 

Modules are registered by type::

    <modules>
        <module type="MyModule" />

You can add nested ``parameters`` and ``properties`` to a module registration in the same manner as for components above.

Additional Config Files
=======================

You can include additional config files using::

    <files>
        <file name="Controllers.config" section="controllers" />

Configuring the Container
=========================

First, you must **reference Autofac.Configuration.dll in from your project**.

To configure the container use a ``ConfigurationSettingsReader`` initialised with the name you gave to your XML configuration section:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterModule(new ConfigurationSettingsReader("mycomponents"));
    // Register other components and call Build() to create the container.

The container settings reader will override default components already registered; you can write your application so that it will run with sensible defaults and then override only those component registrations necessary for a particular deployment.

Multiple Files or Sections
==========================

You can use multiple settings readers in the same container, to read different sections or even different config files if the filename is supplied to the ``ConfigurationSettingsReader`` constructor.