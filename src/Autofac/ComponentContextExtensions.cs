using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac;

public static class ComponentContextExtensions
{
    public static ReflectionCache GetReflectionCache(this IComponentContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return context.ComponentRegistry.ReflectionCache;
    }
}
