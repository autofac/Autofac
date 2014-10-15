====================
Enterprise Library 5
====================

`The Autofac.Extras.EnterpriseLibraryConfigurator package <http://www.nuget.org/packages/Autofac.Extras.EnterpriseLibraryConfigurator/>`_ provides a way to use Autofac as the backing store for dependency injection in `Microsoft Enterprise Library 5 <http://entlib.codeplex.com/releases/view/43135>`_ instead of using Unity. It does this in conjunction with :doc:`the Autofac Common Service Locator implementation <csl>`.

**In Enterprise Library 6, Microsoft removed the tightly-coupled dependency resolution mechanisms from the application blocks so there's no more need for this configurator past Enterprise Library 5.**

Using the Configurator
======================

The simplest way to use the configurator is to set up your Enterprise Library configuration in your ``app.config`` or ``web.config`` and use the ``RegisterEnterpriseLibrary()`` extension. This extension parses the configuration and performs the necessary registrations. You then need to set the ``EnterpriseLibraryContainer.Current`` to use an ``AutofacServiceLocator`` from :doc:`the Autofac Common Service Locator implementation <csl>`.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterEnterpriseLibrary();
    var container = builder.Build();
    var csl = new AutofacServiceLocator(container);
    EnterpriseLibraryContainer.Current = csl;

Specifying a Registration Source
================================

The ``RegisterEnterpriseLibrary()`` extension does allow you to specify your own ``IConfigurationSource`` so if your configuration is not in ``app.config`` or ``web.config`` you can still use Autofac.

.. sourcecode:: csharp

    var config = GetYourConfigurationSource();
    var builder = new ContainerBuilder();
    builder.RegisterEnterpriseLibrary(config);
    var container = builder.Build();
    var csl = new AutofacServiceLocator(container);
    EnterpriseLibraryContainer.Current = csl;
