// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Util.Cache;

/// <summary>
/// An internal variant of <see cref="ReflectionCacheDictionary{TKey, TValue}"/>
/// that handles a tuple key.
/// </summary>
/// <typeparam name="TKey">The member items in the tuple key.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal sealed class ReflectionCacheTupleDictionary<TKey, TValue>
    : ConcurrentDictionary<(TKey, TKey), TValue>, IReflectionCache
    where TKey : MemberInfo
{
    /// <inheritdoc />
    public ReflectionCacheUsage Usage { get; set; } = ReflectionCacheUsage.All;

    /// <inheritdoc />
    public void Clear(ReflectionCacheClearPredicate predicate)
    {
        HashSet<Assembly>? reusableAssemblySet = null;

        foreach (var kvp in this)
        {
            reusableAssemblySet ??= new();

            // Remove an item if *either* member of the tuple matches,
            // for generic implementation type caches where the closed generic
            // is in a different assembly to the open one.
            if (predicate(kvp.Key.Item1, GetKeyAssemblies(kvp.Key.Item1, reusableAssemblySet)) ||
                predicate(kvp.Key.Item2, GetKeyAssemblies(kvp.Key.Item2, reusableAssemblySet)))
            {
                TryRemove(kvp.Key, out _);
            }
        }
    }

    private static IEnumerable<Assembly> GetKeyAssemblies(TKey key, HashSet<Assembly> reusableSet)
    {
        reusableSet.Clear();

        if (key is Type keyType)
        {
            TypeAssemblyReferenceProvider.PopulateAllReferencedAssemblies(keyType, reusableSet);

            return reusableSet;
        }
        else if (key.DeclaringType is Type declaredType)
        {
            TypeAssemblyReferenceProvider.PopulateAllReferencedAssemblies(declaredType, reusableSet);

            return reusableSet;
        }

        throw new InvalidOperationException("Impossible state");
    }
}
