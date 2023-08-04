// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
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
}
