// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core.Activators.Reflection;
using Autofac.Util;
using Autofac.Util.Cache;

namespace Autofac.Core;

/// <summary>
/// Defines known, internal-only caches that are used in relatively hot paths, so we want to
/// avoid the additional dictionary lookup in <see cref="ReflectionCache.GetOrCreateCache(string)"/>.
/// </summary>
internal class InternalReflectionCaches
{
    /// <summary>
    /// Gets the cache used by <see cref="Util.AssemblyExtensions.GetPermittedTypesForAssemblyScanning"/>.
    /// </summary>
    public ReflectionCacheAssemblyDictionary<Assembly, IEnumerable<Type>> AssemblyScanAllowedTypes { get; } = new()
    {
        Usage = ReflectionCacheUsage.Registration,
    };

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericEnumerableInterfaceType"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> IsGenericEnumerableInterface { get; } = new();

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericListOrCollectionInterfaceType"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> IsGenericListOrCollectionInterfaceType { get; } = new();

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericTypeDefinedBy"/>.
    /// </summary>
    public ReflectionCacheTupleDictionary<Type, bool> IsGenericTypeDefinedBy { get; } = new();

    /// <summary>
    /// Gets the cache used by <see cref="ConstructorBinder"/>.
    /// </summary>
    public ReflectionCacheDictionary<ConstructorInfo, Func<object?[], object>> ConstructorBinderFactory { get; } = new();

    /// <summary>
    /// Gets a cache used by <see cref="AutowiringPropertyInjector.InjectProperties"/>.
    /// </summary>
    public ReflectionCacheDictionary<PropertyInfo, Action<object, object?>> AutowiringPropertySetters { get; } = new();

    /// <summary>
    /// Gets a cache used by <see cref="AutowiringPropertyInjector.InjectProperties"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, IReadOnlyList<PropertyInfo>> AutowiringInjectableProperties { get; } = new();

    /// <summary>
    /// Gets a cache used by <see cref="DefaultConstructorFinder"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, ConstructorInfo[]> DefaultPublicConstructors { get; } = new();

    /// <summary>
    /// Gets the set of all internal caches.
    /// </summary>
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
