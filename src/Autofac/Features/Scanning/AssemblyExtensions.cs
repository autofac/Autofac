// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Features.Scanning;

/// <summary>
/// Extensions for assemblies used during assembly scanning operations.
/// </summary>
internal static class AssemblyExtensions
{
    /// <summary>
    /// Get the set of types that Autofac will allow to be loaded from the given assembly.
    /// </summary>
    /// <param name="assembly">The assembly to load types from.</param>
    /// <returns>The set of loadable types.</returns>
    internal static IEnumerable<Type> GetPermittedTypesForAssemblyScanning(this Assembly assembly)
    {
        static IReadOnlyList<Type> Uncached(Assembly assembly)
        {
            return assembly.GetLoadableTypes()
                           .WhichCanBeRegistered()
                           .ToList();
        }

        return ReflectionCacheSet.Shared.Internal.AssemblyScanAllowedTypes.GetOrAdd(assembly, Uncached);
    }

    /// <summary>
    /// Given a set of assemblies, locates all the loadable, registerable types and adds them to the component registry.
    /// </summary>
    /// <param name="assemblies">
    /// The set of assemblies to scan for types.
    /// </param>
    /// <param name="cr">
    /// The registry into which registerable types should be added.
    /// </param>
    /// <param name="rb">
    /// A "template" registration builder that is used to provide activator data
    /// filters and serve as the basis for individual component registrations.
    /// </param>
    internal static void ScanAssemblies(this IEnumerable<Assembly> assemblies, IComponentRegistryBuilder cr, IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb)
    {
        assemblies
            .SelectMany(a => a.GetPermittedTypesForAssemblyScanning())
            .FilterAndRegisterConcreteTypes(cr, rb);
    }
}
