=============================
Passing Parameters to Resolve
=============================

``Resolve`` accepts :doc:`parameters<../register/parameters>` using a variable-length argument list:

.. sourcecode:: csharp

    var fred = Resolve<Person>(new NamedParameter("name", "Fred"));

This will map automatically to the named constructor parameters on the implementation class, if it was registered using Reflection, e.g.:

.. sourcecode:: csharp

    class Person
    {
      public Person(string name)
      ...

Available Parameter Types
-------------------------

Autofac offers several different parameter matching strategies:

 * ``NamedParameter`` - match target parameters by name as above
 * ``TypedParameter`` - match target parameters by type (exact type match required)
 * ``ResolvedParameter`` - flexible parameter matching

``NamedParameter`` and ``TypedParameter`` can supply constant values only.

``ResolvedParameter`` can be used as a way to supply values dynamically retrieved from the container, e.g. by resolving a service by name.

Utilising Parameters from an Expression
---------------------------------------

If ``Person`` is to be registered using an expression, the parameters can be accessed from a second available delegate parameter of type ``IEnumerable<Parameter>``:

.. sourcecode:: csharp

    builder.Register((c, p) => new Person(p.Named<string>("name")));

Delegate Factories
------------------

Check out :doc:`the delegate factory feature<../advanced/delegate-factories>` and the ``Func<T>`` :doc:`relationship type<../resolve/relationships>` for a cleaner way to create components that require parameters.