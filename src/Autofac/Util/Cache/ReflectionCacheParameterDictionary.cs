// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Util.Cache;

/// <summary>
/// A reflection cache dictionary, keyed on a <see cref="ParameterInfo"/>.
/// </summary>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class ReflectionCacheParameterDictionary<TValue>
    : ConcurrentDictionary<ParameterInfo, TValue>, IReflectionCache
{
    /// <inheritdoc />
    public ReflectionCacheUsage Usage { get; set; } = ReflectionCacheUsage.All;

    /// <inheritdoc />
    public void Clear(ReflectionCacheClearPredicate predicate)
    {
        if (predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (Count == 0)
        {
            return;
        }

        var reusableAssemblySet = new HashSet<Assembly>();

        foreach (var kvp in this)
        {
            var member = kvp.Key.Member;
            if (predicate(member, TypeAssemblyReferenceProvider.GetAllReferencedAssemblies(member, reusableAssemblySet)))
            {
                TryRemove(kvp.Key, out _);
            }
        }
    }
}
