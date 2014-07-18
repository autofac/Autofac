=============================
Passing Parameters to Resolve
=============================

When it's time to :doc:`resolve services <../index>`, you may find that you need to pass parameters to the resolution. (If you know the values at registration time, :doc:`you can provide them in the registration instead <../register/parameters>`.)

The ``Resolve()`` methods accept :doc:`the same parameter types available at registration time <../register/parameters>` using a variable-length argument list. Alternatively, :doc:`delegate factories <../advanced/delegate-factories>` and the ``Func<T>`` :doc:`implicit relationship type <../resolve/relationships>` also allow ways to pass parameters during resolution.

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

When you resolve a reflection-based component, the constructor of the type may require a parameter that you need to specify based on a runtime value, something that isn't available at registration time. You can use a parameter in the ``Resolve()`` method call to provide that value.

Say you have a configuration reader that needs a configuration section name passed in:

.. sourcecode:: csharp

    public class ConfigReader : IConfigReader
    {
      public ConfigReader(string configSectionName)
      {
        // Store config section name
      }

      // ...read configuration based on the section name.
    }

You could pass a parameter to the ``Resolve()`` call like this:

.. sourcecode:: csharp

    var reader = scope.Resolve<ConfigReader>(new NamedParameter("configSectionName", "sectionName"));

:doc:`As with registration-time parameters <../register/parameters>`, the ``NamedParameter`` in the example will map to the corresponding named constructor parameter, assuming the ``Person`` component was :doc:`registered using reflection <../register/registration>`.

If you have more than one parameter, just pass them all in via the ``Resolve()`` method:

.. sourcecode:: csharp

    var service = scope.Resolve<AnotherService>(
                    new NamedParameter("id", "service-identifier"),
                    new TypedParameter(typeof(Guid), Guid.NewGuid()),
                    new ResolvedParameter(
                      (pi, ctx) => pi.ParameterType == typeof(ILog) && pi.Name == "logger",
                      (pi, ctx) => LogManager.GetLogger("service")));

Parameters with Lambda Expression Components
============================================

With lambda expression component registrations, you need to add the parameter handling inside your lambda expression so when the ``Resolve()`` call passes them in, you can take advantage of them.

In the component registration expression, you can make use of the incoming parameters by changing the delegate signature you use for registration. Instead of just taking in an ``IComponentContext`` parameter, take in an ``IComponentContext`` and an ``IEnumerable<Parameter>``:

.. sourcecode:: csharp

    // Use TWO parameters to the registration delegate:
    // c = The current IComponentContext to dynamically resolve dependencies
    // p = An IEnumerable<Parameter> with the incoming parameter set
    builder.Register((c, p) =>
                     new ConfigReader(p.Named<string>("configSectionName")))
           .As<IConfigReader>();

Now when you resolve the ``IConfigReader``, your lambda will use the parameters passed in:

.. sourcecode:: csharp

    var reader = scope.Resolve<IConfigReader>(new NamedParameter("configSectionName", "sectionName"));

Passing Parameters Without Explicitly Calling Resolve
=====================================================

Autofac supports two features that allow you to automatically generate service "factories" that can take strongly-typed parameter lists that will be used during resolution. This is a slightly cleaner way to create component instances that require parameters.

- :doc:`Delegate Factories <../advanced/delegate-factories>` allow you to define factory delegate methods.
- The ``Func<T>`` :doc:`implicit relationship type <../resolve/relationships>` can provide an automatically-generated factory function.