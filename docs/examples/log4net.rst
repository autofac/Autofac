==========================
log4net Integration Module
==========================

While there is no specific assembly for log4net support, you can easily inject ``log4net.ILog`` values using a very small custom module.

This module is also a good example of how to use :doc:`Autofac modules <../configuration/modules>` for more than simple configuration - they're also helpful for doing some more advanced extensions.

Here's a sample module that configures Autofac to inject ``ILog`` parameters based on the type of the component being activated. This sample module willl handle both constructor and property injection.

.. sourcecode:: csharp

    public class LoggingModule : Autofac.Module
    {
      private static void InjectLoggerProperties(object instance)
      {
        var instanceType = instance.GetType();

        // Get all the injectable properties to set.
        // If you wanted to ensure the properties were only UNSET properties,
        // here's where you'd do it.
        var properties = instanceType
          .GetProperties(BindingFlags.Public | BindingFlags.Instance)
          .Where(p => p.PropertyType == typeof(ILog) && p.CanWrite && p.GetIndexParameters().Length == 0);

        // Set the properties located.
        foreach (var propToSet in properties)
        {
          propToSet.SetValue(instance, LogManager.GetLogger(instanceType), null);
        }
      }

      private static void OnComponentPreparing(object sender, PreparingEventArgs e)
      {
        var t = e.Component.Activator.LimitType;
        e.Parameters = e.Parameters.Union(
          new[]
          {
            new ResolvedParameter((p, i) => p.ParameterType == typeof(ILog), (p, i) => LogManager.GetLogger(t)),
          });
      }

      protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
      {
        // Handle constructor parameters.
        registration.Preparing += OnComponentPreparing;

        // Handle properties.
        registration.Activated += (sender, e) => InjectLoggerProperties(e.Instance);
      }
    }

**Performance Note**: At the time of this writing, calling ``LogManager.GetLogger(type)`` has a slight performance hit as the internal log manager locks the collection of loggers to retrieve the appropriate logger. An enhancement to the module would be to add caching around logger instances so you can reuse them without the lock hit in the ``LogManager`` call.

Thanks for the original idea/contribution by Rich Tebb/Bailey Ling where the idea was posted `on the Autofac newsgroup <https://groups.google.com/forum/#!msg/autofac/Qb-dVPMbna0/s-jLeWeST3AJ>`_.
