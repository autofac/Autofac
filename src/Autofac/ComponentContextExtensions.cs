using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Util.Cache;

namespace Autofac;

public static class ComponentContextExtensions
{
    public static IReflectionCache GetReflectionCache(this IComponentContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return context.ComponentRegistry.ReflectionCache;
    }
}
