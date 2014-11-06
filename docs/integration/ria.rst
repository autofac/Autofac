=====================
RIA / Domain Services
=====================

Domain Services integration requires the `Autofac.Extras.DomainServices NuGet package <http://www.nuget.org/packages/Autofac.Extras.DomainServices/>`_.

.. contents::
  :local:

Quick Start
===========
To get Autofac integrated with RIA/domain services app you need to reference the Domain Services integration NuGet package, register services, and register the integration module.

.. sourcecode:: csharp

    public class Global : HttpApplication, IContainerProviderAccessor
    {
      // The IContainerProviderAccessor and IContainerProvider
      // interfaces are part of the web integration and are used
      // for registering/resolving dependencies on a per-request
      // basis.
      private static IContainerProvider _containerProvider;

      public IContainerProvider ContainerProvider
      {
        get { return _containerProvider; }
      }

      protected void Application_Start(object sender, EventArgs e)
      {
        var builder = new ContainerBuilder();

        // Register your domain services.
        builder
          .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
          .AssignableTo<DomainService>();

        // Add the RIA Services module so the "Initialize"
        // method gets called on your resolved services.
        builder.RegisterModule<AutofacDomainServiceModule>();

        // Build the container and set the container provider
        // as in standard web integration.
        var container = builder.Build();
        _containerProvider = new ContainerProvider(container);

        // Replace the DomainService.Factory with
        // AutofacDomainServiceFactory so things get resolved.
        var factory = new AutofacDomainServiceFactory(_containerProvider);
        DomainService.Factory = factory;
      }
    }

When you write your domain services, use constructor injection and other standard patterns just like any other Autofac/IoC usage.

Example Implementation
======================

`The Autofac source <https://github.com/autofac/Autofac>`_ contains a demo application project called ``DomainServicesExample`` that is consumed by the ``Remember.Web`` example project. It demonstrates how to integrate Autofac with a Domain Services project.
