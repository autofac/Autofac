// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Util.Cache;

/// <summary>
/// Helper class to determine the set of all assemblies referenced by a type, including indirect references via generic arguments and base classes.
/// </summary>
internal static class TypeAssemblyReferenceProvider
{
    /// <summary>
    /// Get all distinct assemblies referenced directly by <see cref="MemberInfo"/>, if that member is a type,
    /// or the owning type of that member (if it's a field or property).
    /// </summary>
    /// <param name="memberInfo">The member to retrieve references for.</param>
    /// <returns>The set of assemblies.</returns>
    public static IEnumerable<Assembly> GetAllReferencedAssemblies(MemberInfo memberInfo)
    {
        var set = new HashSet<Assembly>();

        GetAllReferencedAssemblies(memberInfo, set);

        return set;
    }

    /// <summary>
    /// Add to a provided <see cref="HashSet{T}"/> all the assemblies referenced directly by a <see cref="MemberInfo"/>, if that member is a type,
    /// or the owning type of that member (if it's a field or property).
    /// </summary>
    /// <param name="memberInfo">The member to retrieve references for.</param>
    /// <param name="holdingSet">A pre-allocated set to use for this purpose.</param>
    /// <remarks>
    /// The holding set is cleared each time this method is called.
    /// </remarks>
    public static IEnumerable<Assembly> GetAllReferencedAssemblies(MemberInfo memberInfo, HashSet<Assembly> holdingSet)
    {
        holdingSet.Clear();

        if (memberInfo is Type keyType)
        {
            PopulateAllReferencedAssemblies(keyType, holdingSet);
        }
        else if (memberInfo.DeclaringType is Type declaredType)
        {
            PopulateAllReferencedAssemblies(declaredType, holdingSet);
        }

        return holdingSet;
    }

    /// <summary>
    /// Add to a provided <see cref="HashSet{T}"/> all assemblies referenced by a given type.
    /// </summary>
    /// <param name="inputType">The type to retrieve references for.</param>
    /// <param name="holdingSet">A set to add any assemblies to.</param>
    private static void PopulateAllReferencedAssemblies(Type inputType, HashSet<Assembly> holdingSet)
    {
        if (inputType.IsArray && inputType.GetElementType() is Type elementType)
        {
            PopulateAllReferencedAssemblies(elementType, holdingSet);
        }

        var genericArguments = inputType.GenericTypeArguments;

        foreach (var genericArgumentType in genericArguments)
        {
            PopulateAllReferencedAssemblies(genericArgumentType, holdingSet);
        }

        holdingSet.Add(inputType.Assembly);

        if (inputType.BaseType is not null && inputType.BaseType != typeof(object))
        {
            PopulateAllReferencedAssemblies(inputType.BaseType, holdingSet);
        }
    }
}
