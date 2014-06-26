=======================
Adapters and Decorators
=======================

Adapters
--------

The `adapter pattern <http://en.wikipedia.org/wiki/Adapter_pattern>`_ takes one service contract and adapts it (like a wrapper) to another.

This `introductory article <http://nblumhardt.com/2010/04/lightweight-adaptation-%E2%80%93-coming-soon/>`_ describes a concrete example of the adapter pattern and how you can work with it in Autofac.

Autofac provides built-in adapter registration so you can register a set of services and have them each automatically adapted to a different interface.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Register the services to be adapted
    builder.RegisterType<SaveCommand>()
           .As<ICommand>()
           .WithMetadata("Name", "Save File");
    builder.RegisterType<OpenCommand>()
           .As<ICommand>()
           .WithMetadata("Name", "Open File");

    // Then register the adapter. In this case, the ICommand
    // registrations are using some metadata, so we're
    // adapting Meta<ICommand> instead of plain ICommand.
    builder.RegisterAdapter<Meta<ICommand>, ToolbarButton>(
       cmd => new ToolbarButton(cmd.Value, (string)cmd.Metadata["Name"]));

    var container = builder.Build();

    // The resolved set of buttons will have two buttons
    // in it - one button adapted for each of the registered
    // ICommand instances.
    var buttons = container.Resolve<IEnumerable<ToolbarButton>>();

Decorators
----------

The `decorator pattern <http://en.wikipedia.org/wiki/Decorator_pattern>`_ is somewhat similar to the adapter pattern, where one service "wraps" another. However, in contrast to adapters, decorators expose the *same service* as what they're decorating. The point of using decorators is to add functionality to an object without changing the object's signature.

This `article <http://nblumhardt.com/2011/01/decorator-support-in-autofac-2-4/>`_ has some details about how decorators work in Autofac.

Autofac provides built-in decorator registration so you can register services and have them automatically wrapped with decorator classes.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Register the services to be decorated. You have to
    // name them rather than register them As<ICommandHandler>()
    // so the *decorator* can be the As<ICommandHandler>() registration.
    builder.RegisterType<SaveCommandHandler>()
           .Named<ICommandHandler>("handler");
    builder.RegisterType<OpenCommandHandler>()
           .Named<ICommandHandler>("handler");

    // Then register the decorator. The decorator uses the
    // named registrations to get the items to wrap.
    builder.RegisterDecorator<ICommandHandler>(
        (c, inner) => new CommandHandlerDecorator(inner),
        fromKey: "handler");

    var container = builder.Build();

    // The resolved set of commands will have two items
    // in it, both of which will be wrapped in a CommandHandlerDecorator.
    var handlers = container.Resolve<IEnumerable<ICommandHandler>>();

You can also use open generic decorator registrations.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Register the open generic with a name so the
    // decorator can use it.
    builder.RegisterGeneric(typeof(CommandHandler<>))
           .Named("handler", typeof(ICommandHandler<>));

    // Register the generic decorator so it can wrap
    // the resolved named generics.
    builder.RegisterGenericDecorator(
            typeof(CommandHandlerDecorator<>),
            typeof(ICommandHandler<>),
            fromKey: "handler");

    var container = builder.Build();

    // You can then resolve closed generics and they'll be
    // wrapped with your decorator.
    var mailHandlers = container.Resolve<IEnumerable<ICommandHandler<EmailCommand>>>();