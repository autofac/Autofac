﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;

namespace Autofac.Util;

/// <summary>
/// Extension methods for <see cref="System.Reflection.Assembly"/>.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Safely returns the set of loadable types from an assembly.
    /// </summary>
    /// <param name="assembly">The <see cref="System.Reflection.Assembly"/> from which to load types.</param>
    /// <returns>
    /// The set of types from the <paramref name="assembly" />, or the subset
    /// of types that could be loaded if there was any error.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="assembly" /> is <see langword="null" />.
    /// </exception>
    private static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        // Algorithm from StackOverflow answer here:
        // https://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        try
        {
            return assembly.DefinedTypes.Select(t => t.AsType());
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }

    private static readonly ConcurrentDictionary<Assembly, IEnumerable<Type>> PossibleAssemblyTypes = new();

    /// <summary>
    /// A first filter for possible assembly types to register.
    /// </summary>
    /// <param name="assembly">The <see cref="System.Reflection.Assembly"/> from which to load types.</param>
    public static IEnumerable<Type> PossibleScanTypes(this Assembly assembly) =>
        PossibleAssemblyTypes.GetOrAdd(assembly, ass =>
            ass.GetLoadableTypes().Where(t => t.IsClass &&
                                              !t.IsAbstract &&
                                              !t.IsDelegate()).ToArray());
}
