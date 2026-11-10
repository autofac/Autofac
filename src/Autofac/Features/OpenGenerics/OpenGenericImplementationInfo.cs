// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Features.OpenGenerics;

/// <summary>
/// The reflection data about an open generic implementation type that binding reads for every closed service.
/// Reflection returns a new array on every call, so this reads it from the type once and caches it.
/// </summary>
internal sealed class OpenGenericImplementationInfo
{
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2070:UnrecognizedReflectionPattern",
        Justification = "Inspects the interfaces of the open generic implementation type to map its generic arguments. The implementation type comes from an open generic registration whose consumer is responsible for preserving its interfaces.")]
    private OpenGenericImplementationInfo(Type implementationType)
    {
        GenericArguments = implementationType.GetGenericArguments();
        GenericParameterConstraints = new Type[GenericArguments.Length][];
        for (var i = 0; i < GenericArguments.Length; i++)
        {
            GenericParameterConstraints[i] = GenericArguments[i].GetGenericParameterConstraints();
        }

        Interfaces = implementationType.GetInterfaces();
        InterfaceGenericArguments = new Type[Interfaces.Length][];
        for (var i = 0; i < Interfaces.Length; i++)
        {
            InterfaceGenericArguments[i] = Interfaces[i].GenericTypeArguments;
        }

        var genericBaseTypes = new List<(Type Definition, Type[] GenericArguments)>();
        for (var baseType = implementationType.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType)
            {
                genericBaseTypes.Add((baseType.GetGenericTypeDefinition(), baseType.GenericTypeArguments));
            }
        }

        GenericBaseTypes = genericBaseTypes.ToArray();
    }

    /// <summary>
    /// Gets the generic parameters of the implementation type.
    /// </summary>
    public Type[] GenericArguments
    {
        get;
    }

    /// <summary>
    /// Gets the type constraints of each generic parameter.
    /// </summary>
    public Type[][] GenericParameterConstraints
    {
        get;
    }

    /// <summary>
    /// Gets the interfaces the implementation type implements.
    /// </summary>
    public Type[] Interfaces
    {
        get;
    }

    /// <summary>
    /// Gets the generic arguments of each interface in <see cref="Interfaces"/>.
    /// </summary>
    public Type[][] InterfaceGenericArguments
    {
        get;
    }

    /// <summary>
    /// Gets the generic base types of the implementation type, nearest first, with their generic arguments.
    /// </summary>
    public (Type Definition, Type[] GenericArguments)[] GenericBaseTypes
    {
        get;
    }

    /// <summary>
    /// Gets the cached reflection data for an open generic implementation type.
    /// </summary>
    /// <param name="implementationType">The open generic implementation type.</param>
    /// <returns>The reflection data.</returns>
    public static OpenGenericImplementationInfo For(Type implementationType)
        => ReflectionCacheSet.Shared.Internal.OpenGenericImplementationInfoByType.GetOrAdd(implementationType, static type => new OpenGenericImplementationInfo(type));
}
