// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;

namespace Autofac.Util;

/// <summary>
/// Delegate for predicates that can choose whether to remove a member from the
/// reflection cache.
/// </summary>
/// <param name="member">
/// The member information (will be an instance of a more-derived type).
/// </param>
/// <returns>
/// True to remove the member from the cache, false to leave it.
/// </returns>
public delegate bool ReflectionCacheShouldClearPredicate(MemberInfo member);

/// <summary>
/// Delegate for callbacks invoked when <see cref="ReflectionCache.Clear"/> is
/// called.
/// </summary>
/// <param name="predicate">
/// An optional predicate to determine which cache items should be cleared.
/// </param>
public delegate void ReflectionCacheRegistrationCallback(ReflectionCacheShouldClearPredicate? predicate);

/// <summary>
/// Provides methods to support clearing Autofac's internal reflection caches.
/// </summary>
public static class ReflectionCache
{
    private static List<ReflectionCacheRegistrationCallback> _cacheRegistrations = new();

    /// <summary>
    /// Register a callback to be invoked when <see cref="Clear"/> is called.
    /// </summary>
    /// <param name="registrationCallback">The callback to invoke.</param>
    /// <remarks>
    /// The owner of the callback should only ever be a static readonly field,
    /// since there is no way to un-register a callback.
    /// </remarks>
    public static void Register(ReflectionCacheRegistrationCallback registrationCallback)
    {
        lock (_cacheRegistrations)
        {
            _cacheRegistrations.Add(registrationCallback);
        }
    }

    /// <summary>
    /// Clear the internal reflection cache. Only call this method if you are
    /// dynamically unloading types from the process; calling this method
    /// otherwise will only slow down Autofac during normal operation.
    /// </summary>
    /// <param name="predicate">
    /// A predicate that can be used to filter which items get removed from the
    /// cache.
    /// </param>
    /// <remarks>
    /// This method is non-deterministic; there is no guarantee that the cache
    /// will be empty by the time the method exits, or that all items matched by
    /// the provided predicate have been removed.
    /// </remarks>
    public static void Clear(ReflectionCacheShouldClearPredicate? predicate = null)
    {
        lock (_cacheRegistrations)
        {
            foreach (var reg in _cacheRegistrations)
            {
                reg(predicate);
            }
        }
    }
}
