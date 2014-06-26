=====================
Circular Dependencies
=====================

Circular dependencies are mutual runtime dependencies between components.

Property/Property Dependencies
------------------------------

This is when you have one class (``DependsByProperty1``) that takes a property dependency of a second type (``DependsByProperty2``), and the second type (``DependsByProperty2``) has a property dependency of the first type (``DependsByProperty1``).

If you have this situation, there are some important things to remember:

 * **Make the property dependencies settable.** The properties must be writeable.
 * **Register the types using** ``PropertiesAutowired``. Be sure to set the behavior to allow circular dependencies.
 * **Neither type can be registered as** ``InstancePerDependency``. If either type is set to factory scope you won't get the results you're looking for (where the two types refer to each other). You can scope them however you like - ``SingleInstance``, ``InstancePerLifetimeScope``, or any other scope - just not factory.

Example:

.. sourcecode:: csharp

    class DependsByProp1
    {
      public DependsByProp2 Dependency { get; set; }
    }

    class DependsByProp2
    {
      public DependsByProp1 Dependency { get; set; }
    }

    // ...

    var cb = new ContainerBuilder();
    cb.RegisterType<DependsByProp1>()
      .InstancePerLifetimeScope()
      .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
    cb.RegisterType<DependsByProp2>()
      .InstancePerLifetimeScope()
      .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

Constructor/Property Dependencies
---------------------------------

This is when you have one class (``DependsByCtor``) that takes a constructor dependency of a second type (``DependsByProperty``), and the second type (``DependsByProperty``) has a property dependency of the first type (``DependsByCtor``).

If you have this situation, there are some important things to remember:

 * **Make the property dependency settable.** The property on the type with the property dependency must be writeable.
 * **Register the type with the property dependency using** ``PropertiesAutowired``. Be sure to set the behavior to allow circular dependencies.
 * **Neither type can be registered as** ``InstancePerDependency``. If either type is set to factory scope you won't get the results you're looking for (where the two types refer to each other). You can scope them however you like - ``SingleInstance``, ``InstancePerLifetimeScope``, or any other scope - just not factory.

Example:

.. sourcecode:: csharp

    class DependsByCtor
    {
      public DependsByCtor(DependsByProp dependency) { }
    }

    class DependsByProp
    {
      public DependsByCtor Dependency { get; set; }
    }

    // ...

    var cb = new ContainerBuilder();
    cb.RegisterType<DependsByCtor>()
      .InstancePerLifetimeScope();
    cb.RegisterType<DependsByProperty>()
      .InstancePerLifetimeScope()
      .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

Constructor/Constructor Dependencies
------------------------------------

Two types with circular constructor dependencies are **not supported**. You will get an exception when you try to resolve types registered in this manner.

You may be able to work around this using the DynamicProxy2 extension and some creative coding.