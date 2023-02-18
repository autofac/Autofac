// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace Autofac.Core;

/// <summary>
/// Delegate for predicates that can choose whether to remove a member from the
/// reflection cache.
/// </summary>
/// <param name="member">
/// The member information (will be an instance of a more-derived type). This
/// value may be null if the cache entry relates only to an assembly.
/// </param>
/// <param name="referencedAssemblies">
/// All assemblies the cache entry key references; this set includes both the direct assembly reference for the member,
/// and all indirectly-referenced assemblies via generic type arguments or array element types.
/// </param>
/// <returns>
/// True to remove the member from the cache, false to leave it.
/// </returns>
public delegate bool ReflectionCacheClearPredicate(MemberInfo? member, IEnumerable<Assembly> referencedAssemblies);

/// <summary>
/// Defines an individual store of cached reflection data.
/// </summary>
public interface IReflectionCache
{
    /// <summary>
    /// Gets a value indicating when the cache is used.
    /// </summary>
    ReflectionCacheUsage Usage { get; }

    /// <summary>
    /// Clear the cache.
    /// </summary>
    void Clear();

    /// <summary>
    /// Conditionally clear the cache, based on the provided predicate.
    /// </summary>
    /// <param name="predicate">A predicate that returns true for cache entries that should be cleared.</param>
    void Clear(ReflectionCacheClearPredicate predicate);
}
