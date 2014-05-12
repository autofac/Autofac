==================
Resolving Services
==================

After you have your :doc:`components registered with appropriate services exposed <../register/index>`, you can resolve services from the built container and child :doc:`lifetime scopes <../lifetime/index>`. You do this using the ``Resolve()`` method::

    var builder = new ContainerBuilder();
    builder.RegisterType<MyComponent>().As<IService>();
    var container = builder.Build();

    using(var scope = container.BeginLifetimeScope())
    {
      var service = scope.Resolve<IService>();
    }

You will notice the example resolves the service from a lifetime scope rather than the container directly - you should, too.

    **While it is possible to resolve components right from the root container, doing this through your application in some cases may result in a memory leak.** It is recommended you always resolve components from a lifetime scope where possible to make sure service instances are properly disposed and garbage collected. You can read more about this in the :doc:`section on controlling scope and lifetime <../lifetime/index>`.

When resolving a service, Autofac will automatically chain down the entire dependency hierarchy of the service and resolve any dependencies required to fully construct the service. If you have :doc:`circular dependencies <../advanced/circular-dependencies>` that are improperly handled or if there are missing required dependencies, you will get a ``DependencyResolutionException``.

If you have a service that may or may not be registered, you can attempt conditional resolution of the service using ``ResolveOptional()`` or ``TryResolve()``::

    // If IService is registered, it will be resolved; if
    // it isn't registered, the return value will be null.
    var service = scope.ResolveOptional<IService>();

    // If IProvider is registered, the provider variable
    // will hold the value; otherwise you can take some
    // other action.
    IProvider provider = null;
    if(scope.TryResolve<IProvider>(out provider))
    {
      // Do something with the resolved provider value.
    }

Both ``ResolveOptional()`` and ``TryResolve()`` revolve around the conditional nature of a specific service *being registered*. If the service is registered, resolution will be attempted. If resolution fails (e.g., due to lack of a dependency being registered), **you will still get a DependencyResolutionException**. If you need conditional resolution around a service where the condition is based on whether or not the service can successfully resolve, wrap the ``Resolve()`` call with a try/catch block.

Additional topics for resolving services:

.. toctree::

    parameters.rst
    relationships.rst

You may also be interested in checking out the list of :doc:`advanced topics <../advanced/index>` to learn about :doc:`named and keyed services <../advanced/keyed-services>`, :doc:`working with component metadata <../advanced/metadata>`, and other service resolution related topics.