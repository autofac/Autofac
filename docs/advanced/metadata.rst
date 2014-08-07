=======================================
Component Metadata / Attribute Metadata
=======================================

If you’re familiar with the Managed Extensibility Framework (MEF) you have probably seen examples using component metadata.

Metadata is information about a component, stored with that component, accessible without necessarily creating a component instance.

Adding Metadata to a Component Registration
===========================================

Values describing metadata are associated with the component at registration time. Each metadata item is a name/value pair:

.. sourcecode:: csharp

    builder.Register(c => new ScreenAppender())
        .As<ILogAppender>()
        .WithMetadata("AppenderName", "screen");

The same thing can be represented in XML:

.. sourcecode:: xml

    <component
       type="MyApp.Components.Logging.ScreenAppender, MyApp"
       service="MyApp.Services.Logging.ILogAppender, MyApp" >
        <metadata>
            <item name="AppenderName" value="screen" type="System.String" />
        </metadata>
    </component>

Consuming Metadata
==================

Unlike a regular property, a metadata item is independent of the component itself.

This makes it useful when selecting one of many components based on runtime criteria; or, where the metadata isn’t intrinsic to the component implementation. Metadata could represent the time that an ``ITask`` should run, or the button caption for an ``ICommand``.

Other components can consume metadata using the ``Meta<T>`` type.

.. sourcecode:: csharp

    public class Log
    {
      readonly IEnumerable<Meta<ILogAppender>> _appenders;

      public Log(IEnumerable<Meta<ILogAppender>> appenders)
      {
        _appenders = appenders;
      }

      public void Write(string destination, string message)
      {
        var appender = _appenders.First(a => a.Metadata["AppenderName"].Equals( destination));
        appender.Value.Write(message);
      }
    }

To consume metadata without creating the target component, use `Meta<Lazy<T>>` or the .NET 4 `Lazy<T, TMetadata>` types as shown below.

Strongly-Typed Metadata
=======================

To avoid the use of string-based keys for describing metadata, a metadata class can be defined with a public read/write property for every metadata item:

.. sourcecode:: csharp

    public class AppenderMetadata
    {
      public string AppenderName { get; set; }
    }

At registration time, the class is used with the overloaded ``WithMetadata`` method to associate values:

.. sourcecode:: csharp

    builder.Register(c => new ScreenAppender())
        .As<ILogAppender>()
        .WithMetadata<AppenderMetadata>(m =>
            m.For(am => am.AppenderName, "screen"));

Notice the use of the strongly-typed ``AppenderName`` property.

Registration and consumption of metadata are separate, so strongy-typed metadata can be consumed via the weakly-typed techniques and vice-versa.

You can also provide default values using the ``DefaultValue`` attribute:

.. sourcecode:: csharp

    public class AppenderMetadata
    {
      [DefaultValue("screen")]
      public string AppenderName { get; set; }
    }

If you are able to reference ``System.ComponentModel.Composition`` you can use the ``System.Lazy<T, TMetadata>`` type for consuming values from the strongly-typed metadata class:

.. sourcecode:: csharp

    public class Log
    {
      readonly IEnumerable<Lazy<ILogAppender, LogAppenderMetadata>> _appenders;

      public Log(IEnumerable<Lazy<ILogAppender, LogAppenderMetadata>> appenders)
      {
        _appenders = appenders;
      }

      public void Write(string destination, string message)
      {
        var appender = _appenders.First(a => a.Metadata.AppenderName == destination);
        appender.Value.Write(message);
      }
    }

Another neat trick is the ability to pass the metadata dictionary into the constructor of your metadata class:

.. sourcecode:: csharp

    public class AppenderMetadata
    {
      public AppenderMetadata(IDictionary<string, object> metadata)
      {
        AppenderName = (string)metadata["AppenderName"];
      }

      public string AppenderName { get; set; }
    }

Interface-Based Metadata
========================

If you have access to ``System.ComponentModel.Composition`` and include a reference to the :doc:`Autofac.Mef <../integration/mef>` package you can use an interface for your metadata instead of a class.

The interface should be defined with a readable property for every metadata item:

.. sourcecode:: csharp

    public interface IAppenderMetadata
    {
      string AppenderName { get; }
    }

You must also call the ``RegisterMetadataRegistrationSources`` method on the ``ContainerBuilder`` before registering the metadata against the interface type.

.. sourcecode:: csharp

    builder.RegisterMetadataRegistrationSources();

At registration time, the interface is used with the overloaded ``WithMetadata`` method to associate values:

.. sourcecode:: csharp

    builder.Register(c => new ScreenAppender())
        .As<ILogAppender>()
        .WithMetadata<IAppenderMetadata>(m =>
            m.For(am => am.AppenderName, "screen"));

Resolving the value can be done in the same manner as for class based metadata.

Attribute-Based Metadata
========================

The ``Autofac.Extras.Attributed`` package enables metadata to be specified via attributes as well as allowing components to filter incoming dependencies using attributes.

To get attributed metadata working in your solution, you need to perform the following steps:

#. :ref:`create_attribute`
#. :ref:`apply_attribute`
#. :ref:`use_filters`
#. :ref:`container_use_attributes`

.. _create_attribute:

Create Your Metadata Attribute
------------------------------

A metadata attribute is a ``System.Attribute`` implementation that has the `System.ComponentModel.Composition.MetadataAttributeAttribute <http://msdn.microsoft.com/en-us/library/system.componentmodel.composition.metadataattributeattribute.aspx>`_ applied.

Any publicly-readable properties on the attribute will become name/value attribute pairs - the name of the metadata will be the property name and the value will be the property value.

In the example below, the ``AgeMetadataAttribute`` will provide a name/value pair of metadata where the name will be ``Age`` (the property name) and the value will be whatever is specified in the attribute during construction.

.. sourcecode:: csharp

    [MetadataAttribute]
    public class AgeMetadataAttribute : Attribute
    {
      public int Age { get; private set; }

      public AgeMetadataAttribute(int age)
      {
        Age = age;
      }
    }

.. _apply_attribute:

Apply Your Metadata Attribute
-----------------------------

Once you have a metadata attribute, you can apply it to your component types to provide metadata.

.. sourcecode:: csharp

    // Don't apply it to the interface (service type)
    public interface IArtwork
    {
      void Display();
    }

    // Apply it to the implementation (component type)
    [AgeMetadata(100)]
    public class CenturyArtwork : IArtwork
    {
      public void Display() { ... }
    }

.. _use_filters:

Use Metadata Filters on Consumers
---------------------------------

Along with providing metadata via attributes, you can also set up automatic filters for consuming components. This will help wire up parameters for your constructors based on provided metadata.

You can filter based on :doc:`a service key <keyed-services>` or based on registration metadata.

WithKeyAttribute
""""""""""""""""

The ``WithKeyAttribute`` allows you to select a specific keyed service to consume.

This example shows a class that requires a component with a particular key:

.. sourcecode:: csharp

    public class ArtDisplay : IDisplay
    {
      public ArtDisplay([WithKey("Painting")] IArtwork art) { ... }
    }

That component will require you to register a keyed service with the specified name. You'll also need to register the component with the filter so the container knows to look for it.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Register the keyed service to consume
    builder.RegisterType<MyArtwork>().Keyed<IArtwork>("Painting");

    // Specify WithAttributeFilter for the consumer
    builder.RegisterType<ArtDisplay>().As<IDisplay>().WithAttributeFilter();

    // ...
    var container = builder.Build();

WithMetadataAttribute
"""""""""""""""""""""

The ``WithMetadataAttribute`` allows you to filter for components based on specific metadata values.

This example shows a class that requires a component with a particular metadata value:

.. sourcecode:: csharp

    public class ArtDisplay : IDisplay
    {
      public ArtDisplay([WithMetadata("Age", 100)] IArtwork art) { ... }
    }

That component will require you to register a service with the specified metadata name/value pair. You could use the attributed metadata class seen in earlier examples, or manually specify metadata during registration time. You'll also need to register the component with the filter so the container knows to look for it.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Register the service to consume with metadata.
    // Since we're using attributed metadata, we also
    // need to register the AttributedMetadataModule
    // so the metadata attributes get read.
    builder.RegisterModule<AttributedMetadataModule>();
    builder.RegisterType<CenturyArtwork>().As<IArtwork>();

    // Specify WithAttributeFilter for the consumer
    builder.RegisterType<ArtDisplay>().As<IDisplay>().WithAttributeFilter();

    // ...
    var container = builder.Build();

.. _container_use_attributes:

Ensure the Container Uses Your Attributes
-----------------------------------------

The metadata attributes you create aren't just used by default. In order to tell the container that you're making use of metadata attributes, you need to register the ``AttributedMetadataModule`` into your container.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Register the service to consume with metadata.
    // Since we're using attributed metadata, we also
    // need to register the AttributedMetadataModule
    // so the metadata attributes get read.
    builder.RegisterModule<AttributedMetadataModule>();
    builder.RegisterType<CenturyArtwork>().As<IArtwork>();

    // ...
    var container = builder.Build();

If you're using metadata filters (``WithKeyAttribute`` or ``WithMetadataAttribute`` in your constructors), you need to register those components using the ``WithAttributeFilter`` extension. Note that if you're *only* using filters but not attributed metadata, you don't actually need the ``AttributedMetadataModule``. Metadata filters stand on their own.

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Specify WithAttributeFilter for the consumer
    builder.RegisterType<ArtDisplay>().As<IDisplay>().WithAttributeFilter();
    // ...
    var container = builder.Build();
