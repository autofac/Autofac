=====================================
Managed Extensibility Framework (MEF)
=====================================

[TODO: Add documentation about ``RegisterMetadataRegistrationSources()``]

The Autofac MEF integration allows you to expose extensibility points in your applications using the `Managed Extensibility Framework <http://msdn.microsoft.com/en-us/library/dd460648(VS.100).aspx>`_.

To use MEF in an Autofac application, you must reference the .NET framework ``System.ComponentModel.Composition.dll`` assembly and get the `Autofac.Mef <http://www.nuget.org/packages/Autofac.Mef/>`_ package from NuGet.

Note this is a one-way operation - it allows Autofac to resolve items that were registered in MEF, but it doesn't allow MEF to resolve items that were registered in Autofac. 

Consuming MEF Extensions in Autofac
===================================

The Autofac/MEF integration allows MEF catalogs to be registered with the ``ContainerBuilder``, then use the ``RegisterComposablePartCatalog()`` extension method.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    var catalog = new DirectoryCatalog(@"C:\MyExtensions");
    builder.RegisterComposablePartCatalog(catalog);

All MEF catalog types are supported:

* ``TypeCatalog``
* ``AssemblyCatalog``
* ``DirectoryCatalog``

Once MEF catalogs are registered, exports within them can be resolved through the Autofac container or by injection into other components. For example, say you have a class with an export type defined using MEF attributes:

.. sourcecode:: csharp

    [Export(typeof(IService))]
    public class Component : IService { }

Using MEF catalogs, you can register that type. Autofac will find the exported interface and provide the service.

.. sourcecode:: csharp

    var catalog = new TypeCatalog(typeof(Component));
    builder.RegisterComposablePartCatalog(catalog);
    var container = builder.Build();

    // The resolved IService will be implemented
    // by type Component.
    var obj = container.Resolve<IService>();

Providing Autofac Components to MEF Extensions
==============================================

Autofac components aren't automatically available for MEF extensions to import. Which is to say, if you use Autofac to resolve a component that was registered using MEF, only other services registered using MEF will be allowed to satisfy its dependencies.

To provide an Autofac component to MEF, the ``Exported()`` extension method must be used:

.. sourcecode:: csharp

    builder.RegisterType<Component>()
           .Exported(x => x.As<IService>().WithMetadata("SomeData", 42));

Again, this is a one-way operation. It allows Autofac to provide dependencies to MEF components that are registered within Autofac - it doesn't export Autofac registrations to be resolved from a MEF catalog.
