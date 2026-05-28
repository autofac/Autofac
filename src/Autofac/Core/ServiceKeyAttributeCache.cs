// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Util;

namespace Autofac.Core;

/// <summary>
/// Caches lookups for <see cref="ServiceKeyAttribute"/> to avoid repeated reflection scans.
/// </summary>
/// <remarks>
/// This class doesn't actually do the caching itself, but provides a single
/// point of access to the cache for both parameters and properties. This is an
/// important distinction because this class is not responsible for flushing
/// reflection info - that's handled by the <see cref="ReflectionCacheSet"/>.
/// </remarks>
internal static class ServiceKeyAttributeCache
{
    /// <summary>
    /// Determines whether a parameter is decorated with <see cref="ServiceKeyAttribute"/>.
    /// </summary>
    /// <param name="parameter">The parameter to inspect.</param>
    /// <returns><see langword="true"/> when the attribute is present; otherwise <see langword="false"/>.</returns>
    public static bool ParameterHasServiceKey(ParameterInfo parameter)
    {
        if (parameter == null)
        {
            throw new ArgumentNullException(nameof(parameter));
        }

        return ReflectionCacheSet.Shared.Internal.ServiceKeyParameterAttributes.GetOrAdd(
            parameter,
            static p =>
            {
                if (p.IsDefined(typeof(ServiceKeyAttribute), inherit: true))
                {
                    return true;
                }

                return p.TryGetDeclaringProperty(out var property) && PropertyHasServiceKey(property);
            });
    }

    /// <summary>
    /// Determines whether a property is decorated with <see cref="ServiceKeyAttribute"/>.
    /// </summary>
    /// <param name="property">The property to inspect.</param>
    /// <returns><see langword="true"/> when the attribute is present; otherwise <see langword="false"/>.</returns>
    public static bool PropertyHasServiceKey(PropertyInfo property)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        return ReflectionCacheSet.Shared.Internal.ServiceKeyPropertyAttributes.GetOrAdd(
            property,
            static p => p.IsDefined(typeof(ServiceKeyAttribute), inherit: true));
    }
}
