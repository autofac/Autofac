==============================
Passing Parameters to Register
==============================

When you :doc:`register components <registration>` you have the ability to provide a set of parameters that can be used during the :doc:`resolution of services <../resolve/index>` based on that component.

Available Parameter Types
=========================

Autofac offers several different parameter matching strategies:

* ``NamedParameter`` - match target parameters by name
* ``TypedParameter`` - match target parameters by type (exact type match required)
* ``ResolvedParameter`` - flexible parameter matching

``NamedParameter`` and ``TypedParameter`` can supply constant values only.

``ResolvedParameter`` can be used as a way to supply values dynamically retrieved from the container, e.g. by resolving a service by name.

Parameters with Reflection Components
=====================================

When you register a reflection-based component, the constructor of the type may require a parameter that can't be resolved from the container. You can use a parameter on the registration to provide that value.

Say you have a configuration reader that needs a configuration section name passed in::

    public class ConfigReader : IConfigReader
    {
      public ConfigReader(string configSectionName)
      {
        // Store config section name
      }

      // ...read configuration based on the section name.
    }

You could use a lambda expression component for that::

    builder.Register(c => new ConfigReader("sectionName")).As<IConfigReader>();

Or you could pass a parameter to a reflection component registration::

    // Using a NAMED parameter:
    builder.RegisterType<ConfigReader>()
           .As<IConfigReader>()
           .WithParameter("configSectionName", "sectionName");

    // Using a TYPED parameter:
    builder.RegisterType<ConfigReader>()
           .As<IConfigReader>()
           .WithParameter(new TypedParameter(typeof(string), "sectionName"));

    // Using a RESOLVED parameter:
    builder.RegisterType<ConfigReader>()
           .As<IConfigReader>()
           .WithParameter(
             new ResolvedParameter(
               (pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "configSectionName",
               (pi, ctx) => "sectionName"));

Parameters with Lambda Expression Components
============================================

With lambda expression component registrations, rather than passing the parameter value *at registration time* you enable the ability to pass the value *at service resolution time*. (:doc:`Read more about resolving with parameters. <../resolve/parameters>`)

In the component registration expression, you can make use of the incoming parameters by changing the delegate signature you use for registration. Instead of just taking in an ``IComponentContext`` parameter, take in an ``IComponentContext`` and an ``IEnumerable<Parameter>`` ::

    // Use TWO parameters to the registration delegate:
    // c = The current IComponentContext to dynamically resolve dependencies
    // p = An IEnumerable<Parameter> with the incoming parameter set
    builder.Register((c, p) =>
                     new ConfigReader(p.Named<string>("configSectionName")))
           .As<IConfigReader>();

When :doc:`resolving with parameters <../resolve/parameters>`, your lambda will use the parameters passed in::

    var reader = scope.Resolve<IConfigReader>(new NamedParameter("configSectionName", "sectionName"));