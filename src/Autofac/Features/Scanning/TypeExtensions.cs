// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Util;

namespace Autofac.Features.Scanning;

/// <summary>
/// Extension methods for working with types in a scanning context.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// Filters a list of types using the set of filters associated with the provided activator data.
    /// </summary>
    /// <typeparam name="TActivatorData">Activator builder type.</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
    /// <param name="types">The set of types to filter.</param>
    /// <param name="activatorData">The activator data with filters to run on the type list.</param>
    /// <returns>
    /// A filtered list of types that can be registered according to the activator data.
    /// </returns>
    internal static IEnumerable<Type> AllowedByActivatorFilters<TActivatorData, TRegistrationStyle>(this IEnumerable<Type> types, BaseScanningActivatorData<TActivatorData, TRegistrationStyle> activatorData)
        where TActivatorData : ReflectionActivatorData
    {
        // Issue #897: For back compat reasons we can't filter out
        // non-public types here. Folks use assembly scanning on their
        // own stuff, so encapsulation is a tricky thing to manage.
        // If people want only public types, a LINQ Where clause can be used.
        return types.Where(t => activatorData.Filters.All(p => p(t)));
    }

    /// <summary>
    /// Filters a list of types down to only those concrete types allowed by scanning registrations.
    /// </summary>
    /// <param name="types">
    /// The types to check.
    /// </param>
    /// <returns>
    /// A filtered set of types that remove non-concrete types (interfaces, abstract classes, etc.).
    /// </returns>
    internal static IEnumerable<Type> WhichCanBeRegistered(this IEnumerable<Type> types) => types.Where(t => t.IsRegisterableType());

    /// <summary>
    /// Determines if a type is a concrete type that is allowed to be registered during scanning.
    /// </summary>
    /// <param name="type">
    /// The type to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the type is allowed to be registered during scanning based on its reflection attributes; otherwise <see langword="false"/>.
    /// </returns>
    // Run IsCompilerGenerated check last due to perf. See AssemblyScanningPerformanceTests.MeasurePerformance.
    internal static bool IsRegisterableType(this Type? type) => type != null && type.IsClass && !type.IsAbstract && !type.IsDelegate() && !type.IsCompilerGenerated();
}
