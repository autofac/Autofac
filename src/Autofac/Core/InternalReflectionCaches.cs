// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core.Activators.Reflection;
using Autofac.Util;
using Autofac.Util.Cache;

namespace Autofac.Core;

/// <summary>
/// Defines known, internal-only caches that are used in relatively hot paths, so we want to
/// avoid the additional dictionary lookup in <see cref="ReflectionCacheSet.GetOrCreateCache(string)"/>.
/// </summary>
internal class InternalReflectionCaches
{
    /// <summary>
    /// Gets the cache used by <see cref="Util.AssemblyExtensions.GetPermittedTypesForAssemblyScanning"/>.
    /// </summary>
    public ReflectionCacheAssemblyDictionary<Assembly, IEnumerable<Type>> AssemblyScanAllowedTypes { get; }

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericEnumerableInterfaceType"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> IsGenericEnumerableInterface { get; }

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericListOrCollectionInterfaceType"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> IsGenericListOrCollectionInterfaceType { get; }

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericTypeDefinedBy"/>.
    /// </summary>
    public ReflectionCacheTupleDictionary<Type, bool> IsGenericTypeDefinedBy { get; }

    /// <summary>
    /// Gets the cache used by <see cref="ConstructorBinder"/>.
    /// </summary>
    public ReflectionCacheDictionary<ConstructorInfo, Func<object?[], object>> ConstructorBinderFactory { get; }

    /// <summary>
    /// Gets a cache used by <see cref="AutowiringPropertyInjector.InjectProperties"/>.
    /// </summary>
    public ReflectionCacheDictionary<PropertyInfo, Action<object, object?>> AutowiringPropertySetters { get; }

    /// <summary>
    /// Gets a cache used by <see cref="AutowiringPropertyInjector.InjectProperties"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, IReadOnlyList<PropertyInfo>> AutowiringInjectableProperties { get; }

    /// <summary>
    /// Gets a cache used by <see cref="DefaultConstructorFinder"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, ConstructorInfo[]> DefaultPublicConstructors { get; }

    public ReflectionCacheDictionary<Type, Type> GenericTypeDefinitionByType  { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalReflectionCaches"/> class.
    /// </summary>
    /// <param name="set">The cache set used to retrieve the required caches.</param>
    public InternalReflectionCaches(ReflectionCacheSet set)
    {
        AssemblyScanAllowedTypes = set.GetOrCreateCache(nameof(AssemblyScanAllowedTypes), _ => new ReflectionCacheAssemblyDictionary<Assembly, IEnumerable<Type>>
        {
            Usage = ReflectionCacheUsage.Registration,
        });

        IsGenericEnumerableInterface = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>(nameof(IsGenericEnumerableInterface));
        IsGenericListOrCollectionInterfaceType = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>(nameof(IsGenericListOrCollectionInterfaceType));
        IsGenericTypeDefinedBy = set.GetOrCreateCache<ReflectionCacheTupleDictionary<Type, bool>>(nameof(IsGenericTypeDefinedBy));
        ConstructorBinderFactory = set.GetOrCreateCache<ReflectionCacheDictionary<ConstructorInfo, Func<object?[], object>>>(nameof(ConstructorBinderFactory));
        AutowiringPropertySetters = set.GetOrCreateCache<ReflectionCacheDictionary<PropertyInfo, Action<object, object?>>>(nameof(AutowiringPropertySetters));
        AutowiringInjectableProperties = set.GetOrCreateCache<ReflectionCacheDictionary<Type, IReadOnlyList<PropertyInfo>>>(nameof(AutowiringInjectableProperties));
        DefaultPublicConstructors = set.GetOrCreateCache<ReflectionCacheDictionary<Type, ConstructorInfo[]>>(nameof(DefaultPublicConstructors));
        GenericTypeDefinitionByType = set.GetOrCreateCache<ReflectionCacheDictionary<Type, Type>>(nameof(GenericTypeDefinitionByType));
    }
}
