using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Autofac.Util.Cache;

/// <summary>
/// Defines known, internal-only caches that are used in relatively hot paths, so we want to
/// avoid the additional dictionary lookup in <see cref="IReflectionCache.GetOrCreateCache(string)"/>.
/// </summary>
internal class InternalReflectionCaches
{
    public ReflectionCacheAssemblyDictionary<Assembly, IEnumerable<Type>> AssemblyScanAllowedTypes { get; } = new()
    {
        UsedAtRegistrationOnly = true,
    };

    public ReflectionCacheDictionary<Type, bool> IsGenericEnumerableInterface { get; } = new();

    public ReflectionCacheDictionary<Type, bool> IsGenericListOrCollectionInterfaceType { get; } = new();

    public ReflectionCacheTupleDictionary<Type, bool> IsGenericTypeDefinedBy { get; } = new();

    public ReflectionCacheDictionary<ConstructorInfo, Func<object?[], object>> ConstructorBinderFactory { get; } = new();

    public ReflectionCacheDictionary<PropertyInfo, Action<object, object?>> AutowiringPropertySetters { get; } = new();

    public ReflectionCacheDictionary<Type, IReadOnlyList<PropertyInfo>> AutowiringInjectableProperties { get; } = new();

    public ReflectionCacheDictionary<Type, ConstructorInfo[]> DefaultPublicConstructors { get; } = new();

    public IEnumerable<IReflectionCacheStore> All
    {
        get
        {
            yield return AssemblyScanAllowedTypes;
            yield return IsGenericEnumerableInterface;
            yield return IsGenericListOrCollectionInterfaceType;
            yield return IsGenericTypeDefinedBy;
            yield return ConstructorBinderFactory;
            yield return AutowiringPropertySetters;
            yield return AutowiringInjectableProperties;
            yield return DefaultPublicConstructors;
        }
    }
}
