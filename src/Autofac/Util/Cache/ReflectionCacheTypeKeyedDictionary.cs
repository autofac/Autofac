// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Util.Cache;

/// <summary>
/// A reflection cache dictionary, keyed on a <c>(<see cref="Type"/>, <typeparamref name="TDiscriminator"/>)</c>
/// tuple where <typeparamref name="TDiscriminator"/> is a non-assembly-bearing discriminator (e.g. an enum).
/// Only the <see cref="Type"/> component of the key participates in assembly-predicate–based clearing.
/// </summary>
/// <typeparam name="TDiscriminator">
/// The second component of the tuple key; must be non-null but need not be a <see cref="MemberInfo"/>.
/// </typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
internal sealed class ReflectionCacheTypeKeyedDictionary<TDiscriminator, TValue>
    : ConcurrentDictionary<(Type, TDiscriminator), TValue>, IReflectionCache
    where TDiscriminator : notnull
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
            if (predicate(kvp.Key.Item1, TypeAssemblyReferenceProvider.GetAllReferencedAssemblies(kvp.Key.Item1, reusableAssemblySet)))
            {
                TryRemove(kvp.Key, out _);
            }
        }
    }
}
