=================
Assembly Scanning
=================

Autofac can use conventions to find and register components in assemblies. You can scan and register individual types or you can scan specifically for :doc:`Autofac modules <../configuration/modules>`.

Scanning for Types
==================

Otherwise known as convention-driven registration or scanning, Autofac can register a set of types from an assembly according to user-specified rules::

    var dataAccess = Assembly.GetExecutingAssembly();

    builder.RegisterAssemblyTypes(dataAccess)
           .Where(t => t.Name.EndsWith("Repository"))
           .AsImplementedInterfaces();

Each ``RegisterAssemblyTypes()`` call will apply one set of rules only - multiple invocations of ``RegisterAssemblyTypes()`` are necessary if there are multiple different sets of components to register.

Filtering Types
---------------

``RegisterAssemblyTypes()`` accepts a parameter array of one or more assemblies. By default, all public, concrete classes in the assembly will be registered. You can filter the set of types to register using some provided LINQ-style predicates.

To filter the types that are registered, use the ``Where()`` predicate::

    builder.RegisterAssemblyTypes(asm)
           .Where(t => t.Name.EndsWith("Repository"));

To exclude types from scanning, use the ``Except()`` predicate::

    builder.RegisterAssemblyTypes(asm)
           .Except<MyUnwantedType>();

The ``Except()`` predicate also allows you to customize the registration for the specific excluded type::

    builder.RegisterAssemblyTypes(asm)
           .Except<MyCustomisedType>(ct =>
              ct.As<ISpecial>().SingleInstance());

Multiple filters can be used, in which case they will be applied with logical AND.

Specifying Services
-------------------

The registration syntax for ``RegisterAssemblyTypes()`` is a superset of :doc:`the registration syntax for single types <index>`, so methods like ``As()`` all work with assemblies as well::

    builder.RegisterAssemblyTypes(asm)
           .Where(t => t.Name.EndsWith("Repository"))
           .As<IRepository>();

Additional overloads to ``As()`` and ``Named()`` accept lambda expressions that determine, for a type, which services it will provide::

    builder.RegisterAssemblyTypes(asm)
           .As(t => t.GetInterfaces()[0]);

As with normal component registrations, multiple calls to ``As()`` are added together.

A number of additional registration methods have been added to make it easier to build up common conventions:

+-------------------------------+---------------------------------------+--------------------------------------------------------+
| Method                        | Description                           | Example                                                |
+===============================+=======================================+========================================================+
| ``AsImplementedInterfaces()`` | Register the type as providing        | ::                                                     |
|                               | all of its public interfaces as       |                                                        |
|                               | services (excluding ``IDisposable``). |      builder.RegisterAssemblyTypes(asm)                |
|                               |                                       |             .Where(t => t.Name.EndsWith("Repository")) |
|                               |                                       |             .AsImplementedInterfaces();                |
+-------------------------------+---------------------------------------+--------------------------------------------------------+
| ``AsClosedTypesOf(open)``     | Register types that are assignable to | ::                                                     |
|                               | a closed instance of the open         |                                                        |
|                               | generic type.                         |      builder.RegisterAssemblyTypes(asm)                |
|                               |                                       |             .AsClosedTypesOf(typeof(IRepository<>));   |
+-------------------------------+---------------------------------------+--------------------------------------------------------+
| ``AsSelf()``                  | The default: register types as        | ::                                                     |
|                               | themselves - useful when also         |                                                        |
|                               | overriding the default with another   |      builder.RegisterAssemblyTypes(asm)                |
|                               | service specification.                |             .AsImplementedInterfaces()                 |
|                               |                                       |             .AsSelf();                                 |
+-------------------------------+---------------------------------------+--------------------------------------------------------+

Scanning for Modules
====================

Module scanning is performed with the ``RegisterAssemblyModules()`` registration method, which does exactly what its name suggests. It scans through the provided assemblies for :doc:`Autofac modules <../configuration/modules>`, creates instances of the modules, and then registers them with the current container builder.

For example, say the two simple module classes below live in the same assembly and each register a single component::

    public class AModule : Module
    {
      protected override void Load(ContainerBuilder builder)
      {
        builder.Register(c => new AComponent()).As<AComponent>();
      }
    }

    public class BModule : Module
    {
      protected override void Load(ContainerBuilder builder)
      {
        builder.Register(c => new BComponent()).As<BComponent>();
      }
    }

The overload of ``RegisterAssemblyModules()`` that *does not accept a type parameter* will register all classes implementing ``IModule`` found in the provided list of assemblies. In the example below **both modules** get registered::

    var assembly = typeof(AComponent).Assembly;
    var builder = new ContainerBuilder();

    // Registers both modules
    builder.RegisterAssemblyModules(assembly);

The overload of ``RegisterAssemblyModules()`` with *the generic type parameter* allows you to specify a base type that the modules must derive from. In the example below **only one module** is registered because the scanning is restricted::

    var assembly = typeof(AComponent).Assembly;
    var builder = new ContainerBuilder();

    // Registers AModule but not BModule
    builder.RegisterAssemblyModules<AModule>(assembly);

The overload of ``RegisterAssemblyModules()`` with *a Type object parameter* works like the generic type parameter overload but allows you to specify a type that might be determined at runtime. In the example below **only one module** is registered because the scanning is restricted::

    var assembly = typeof(AComponent).Assembly;
    var builder = new ContainerBuilder();

    // Registers AModule but not BModule
    builder.RegisterAssemblyModules(typeof(AModule), assembly);

IIS Hosted Web Applications
===========================
When hosting applications in IIS all assemblies are loaded into the ``AppDomain`` when the application first starts, but **when the AppDomain is recycled by IIS the assemblies are then only loaded on demand.**

To avoid this issue use the `GetReferencedAssemblies() <http://msdn.microsoft.com/en-us/library/system.web.compilation.buildmanager.getreferencedassemblies.aspx>`_ method on `System.Web.Compilation.BuildManager <http://msdn.microsoft.com/en-us/library/system.web.compilation.buildmanager.aspx>`_ to get a list of the referenced assemblies instead::

    var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>();

That will force the referenced assemblies to be loaded into the ``AppDomain`` immediately making them available for module scanning.