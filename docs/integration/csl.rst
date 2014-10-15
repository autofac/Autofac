======================
Common Service Locator
======================

The `Autofac.Extras.CommonServiceLocator <http://www.nuget.org/packages/Autofac.Extras.CommonServiceLocator/>`_ package allows you to use Autofac as the backing store for services in places where you require `Microsoft Common Service Locator <http://www.nuget.org/packages/CommonServiceLocator/>`_ integration.

The Autofac.Extras.CommonServiceLocator package will also work in conjunction with the :doc:`Autofac Microsoft Enterprise Library integration package <entlib>`.

To use the Common Service Locator integration, build your Autofac container as normal, then simply set the current service locator to an ``AutofacServiceLocator``.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Perform registrations and build the container.
    var container = builder.Build();

    // Set the service locator to an AutofacServiceLocator.
    var csl = new AutofacServiceLocator(container);
    ServiceLocator.SetLocatorProvider(() => csl);
