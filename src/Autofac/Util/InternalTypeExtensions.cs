// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac.Core;

namespace Autofac.Util;

/// <summary>
/// Provides helper methods for manipulating and inspecting types.
/// </summary>
internal static class InternalTypeExtensions
{
    /// <summary>
    /// For a delegate type, outputs the return type of the delegate.
    /// </summary>
    /// <param name="type">The delegate type.</param>
    /// <returns>The delegate return type.</returns>
    public static Type FunctionReturnType(this Type type)
    {
        var invoke = type.GetDeclaredMethod("Invoke");
        Enforce.NotNull(invoke);
        return invoke.ReturnType;
    }

    /// <summary>
    /// Determine whether a given type is an open generic.
    /// </summary>
    /// <param name="type">The input type.</param>
    /// <returns>True if the type is an open generic; false otherwise.</returns>
    public static bool IsOpenGeneric(this Type type)
    {
        return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
    }

    /// <summary>Returns the first concrete interface supported by the candidate type that
    /// closes the provided open generic service type.</summary>
    /// <param name="this">The type that is being checked for the interface.</param>
    /// <param name="openGeneric">The open generic type to locate.</param>
    /// <returns>The type of the interface.</returns>
    public static IEnumerable<Type> GetTypesThatClose(this Type @this, Type openGeneric)
    {
        return FindAssignableTypesThatClose(@this, openGeneric);
    }

    /// <summary>
    /// Checks whether this type is a closed type of a given generic type.
    /// </summary>
    /// <param name="this">The type we are checking.</param>
    /// <param name="openGeneric">The open generic type to validate against.</param>
    /// <returns>True if <paramref name="this"/> is a closed type of <paramref name="openGeneric"/>. False otherwise.</returns>
    public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
    {
        return TypesAssignableFrom(@this).Any(t => t.IsGenericType && !@this.ContainsGenericParameters && t.GetGenericTypeDefinition() == openGeneric);
    }

    /// <summary>
    /// Determines whether a given generic type definition is compatible with the specified type parameters (i.e. is it possible to create a closed generic type from those parameters).
    /// </summary>
    /// <param name="genericTypeDefinition">The generic type definition.</param>
    /// <param name="parameters">The set of parameters to check against.</param>
    /// <returns>True if the parameters match the generic parameter constraints.</returns>
    public static bool IsCompatibleWithGenericParameterConstraints(this Type genericTypeDefinition, Type[] parameters)
    {
        var genericArgumentDefinitions = genericTypeDefinition.GetGenericArguments();

        for (var i = 0; i < genericArgumentDefinitions.Length; ++i)
        {
            var genericArg = genericArgumentDefinitions[i];
            var parameter = parameters[i];

            if (genericArg.GetGenericParameterConstraints()
                .Select(constraint => SubstituteGenericParameterConstraint(parameters, constraint))
                .Any(constraint => !ParameterCompatibleWithTypeConstraint(parameter, constraint)))
            {
                return false;
            }

            var specialConstraints = genericArg.GenericParameterAttributes;

            if ((specialConstraints & GenericParameterAttributes.DefaultConstructorConstraint)
                != GenericParameterAttributes.None)
            {
                if (!parameter.IsValueType && parameter.GetDeclaredPublicConstructors().All(c => c.GetParameters().Length > 0))
                {
                    return false;
                }
            }

            if ((specialConstraints & GenericParameterAttributes.ReferenceTypeConstraint)
                != GenericParameterAttributes.None)
            {
                if (parameter.IsValueType)
                {
                    return false;
                }
            }

            if ((specialConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint)
                != GenericParameterAttributes.None)
            {
                if (!parameter.IsValueType ||
                    (parameter.IsGenericType && IsGenericTypeDefinedBy(parameter, typeof(Nullable<>))))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether a type is compiler generated.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is compiler generated; false otherwise.</returns>
    public static bool IsCompilerGenerated(this Type type)
    {
        return type.GetCustomAttributes<CompilerGeneratedAttribute>().Any();
    }

    /// <summary>
    /// Checks whether a given type is a delegate type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a delegate; false otherwise.</returns>
    public static bool IsDelegate(this Type type)
    {
        return type.IsSubclassOf(typeof(Delegate));
    }

    /// <summary>
    /// Checks whether a given type is a generic enumerable interface type, e.g. <see cref="IEnumerable{T}" />, <see cref="IList{T}"/>, <see cref="ICollection{T}"/>, etc.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is one of the supported enumerable interface types.</returns>
    public static bool IsGenericEnumerableInterfaceType(this Type type)
    {
        static bool Uncached(Type type)
        {
            return type.IsGenericTypeDefinedBy(typeof(IEnumerable<>))
                   || type.IsGenericListOrCollectionInterfaceType();
        }

        return ReflectionCacheSet.Shared.Internal.IsGenericEnumerableInterface.GetOrAdd(type, Uncached);
    }

    /// <summary>
    /// Checks whether a given type is a generic list of collection interface type, e.g. <see cref="IList{T}"/>, <see cref="ICollection{T}"/> and the read-only variants.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is one of the supported list/collection types.</returns>
    public static bool IsGenericListOrCollectionInterfaceType(this Type type)
    {
        static bool Uncached(Type type)
        {
            return type.IsGenericTypeDefinedBy(typeof(IList<>))
                   || type.IsGenericTypeDefinedBy(typeof(ICollection<>))
                   || type.IsGenericTypeDefinedBy(typeof(IReadOnlyCollection<>))
                   || type.IsGenericTypeDefinedBy(typeof(IReadOnlyList<>));
        }

        return ReflectionCacheSet.Shared.Internal
                              .IsGenericListOrCollectionInterfaceType.GetOrAdd(type, Uncached);
    }

    /// <summary>
    /// Checks whether a given type is a closed generic defined by the specified open generic.
    /// </summary>
    /// <param name="this">The type to check.</param>
    /// <param name="openGeneric">The open generic to check against.</param>
    /// <returns>True if the type is defined by the specified open generic; false otherwise.</returns>
    public static bool IsGenericTypeDefinedBy(this Type @this, Type openGeneric)
    {
        static bool Uncached(Type type, Type openGeneric)
        {
            return !type.ContainsGenericParameters
                    && type.IsGenericType
                    && type.GetGenericTypeDefinition() == openGeneric;
        }

        return ReflectionCacheSet.Shared.Internal.IsGenericTypeDefinedBy.GetOrAdd(
            (@this, openGeneric),
            key => Uncached(key.Item1, key.Item2));
    }

    /// <summary>
    /// Checks whether this type is a generic containing the given type.
    /// </summary>
    /// <param name="this">The type to check.</param>
    /// <param name="type">The type to validate against.</param>
    /// <returns>True if the <paramref name="this"/> is a generic containing <paramref name="type"/>; false otherwise.</returns>
    /// <remarks>Recursively moves through generic type arguments looking for <paramref name="type"/>.</remarks>
    public static bool IsGenericTypeContainingType(this Type @this, Type type)
    {
        static bool Uncached(Type @this, Type type)
        {
            return @this.IsGenericType && @this.GenericTypeArguments.Any(genericType => genericType == type || genericType.IsGenericTypeContainingType(type));
        }

        return ReflectionCacheSet.Shared.Internal.IsGenericTypeContainingType.GetOrAdd(
            (@this, type),
            key => Uncached(key.Item1, key.Item2));
    }

    /// <summary>
    /// Checks whether this type is an open generic type of a given type.
    /// </summary>
    /// <param name="this">The type we are checking.</param>
    /// <param name="type">The type to validate against.</param>
    /// <returns>True if <paramref name="this"/> is a open generic type of <paramref name="type"/>. False otherwise.</returns>
    public static bool IsOpenGenericTypeOf(this Type @this, Type type)
    {
        if (@this == null || type == null)
        {
            return false;
        }

        if (!@this.IsGenericTypeDefinition)
        {
            return false;
        }

        if (@this == type)
        {
            return true;
        }

        return @this.CheckBaseTypeIsOpenGenericTypeOf(type)
          || @this.CheckInterfacesAreOpenGenericTypeOf(type);
    }

    /// <summary>
    /// Filters off interfaces, abstract types, and generally non-registerable
    /// types. Largely used during type/assembly scanning, but also can
    /// determine if this is something we can activate by reflection.
    /// Intentionally does NOT filter out open generic definitions because this
    /// gets used in both concrete and open generic scanning.
    /// </summary>
    /// <param name="type">
    /// The type to check.
    /// </param>
    /// <param name="allowCompilerGenerated">
    /// <see langword="true"/> to allow compiler-generated types to be
    /// considered registerable. Defaults to <see langword="false"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the type is allowed to be registered during
    /// scanning based on its reflection attributes; otherwise <see
    /// langword="false"/>.
    /// </returns>
    // Issue #897: For back compat reasons we can't filter out
    // non-public types here. Folks use assembly scanning on their
    // own stuff, so encapsulation is a tricky thing to manage.
    // If people want only public types, a LINQ Where clause can be used.
    //
    // Run IsCompilerGenerated check last due to perf. See AssemblyScanningPerformanceTests.MeasurePerformance.
    internal static bool MayAllowReflectionActivation(this Type? type, bool allowCompilerGenerated = false) => type is not null && type.IsClass && !type.IsAbstract && !type.IsDelegate() && (allowCompilerGenerated || !type.IsCompilerGenerated());

    private static bool CheckBaseTypeIsOpenGenericTypeOf(this Type @this, Type type)
    {
        if (@this.BaseType == null)
        {
            return false;
        }

        return @this.BaseType.IsGenericType
            ? @this.BaseType.GetGenericTypeDefinition().IsOpenGenericTypeOf(type)
            : type.IsAssignableFrom(@this.BaseType);
    }

    private static bool CheckInterfacesAreOpenGenericTypeOf(this Type @this, Type type)
    {
        return @this.GetInterfaces()
            .Any(it => it.IsGenericType
                ? it.GetGenericTypeDefinition().IsOpenGenericTypeOf(type)
                : type.IsAssignableFrom(it));
    }

    /// <summary>
    /// Looks for an interface on the candidate type that closes the provided open generic interface type.
    /// </summary>
    /// <param name="candidateType">The type that is being checked for the interface.</param>
    /// <param name="openGenericServiceType">The open generic service type to locate.</param>
    /// <returns>True if a closed implementation was found; otherwise false.</returns>
    private static IEnumerable<Type> FindAssignableTypesThatClose(Type candidateType, Type openGenericServiceType)
    {
        return TypesAssignableFrom(candidateType)
            .Where(t => t.IsClosedTypeOf(openGenericServiceType));
    }

    private static Type SubstituteGenericParameterConstraint(Type[] parameters, Type constraint)
    {
        if (!constraint.IsGenericParameter)
        {
            return constraint;
        }

        return parameters[constraint.GenericParameterPosition];
    }

    private static bool ParameterCompatibleWithTypeConstraint(Type parameter, Type constraint)
    {
        if (constraint.IsAssignableFrom(parameter))
        {
            return true;
        }

        var allGenericParametersMatch = false;
        var baseType = parameter.BaseType ?? parameter;
        if (!constraint.IsInterface &&
            baseType.IsGenericType &&
            baseType.GenericTypeArguments.Length > 0 &&
            baseType.GenericTypeArguments.Length == constraint.GenericTypeArguments.Length)
        {
            allGenericParametersMatch = true;
            for (var i = 0; i < baseType.GenericTypeArguments.Length; i++)
            {
                var paramArg = baseType.GenericTypeArguments[i];
                var constraintArg = constraint.GenericTypeArguments[i];
                var constraintArgIsGeneric = constraintArg.IsGenericType;

                allGenericParametersMatch &= paramArg.IsClosedTypeOf(constraintArgIsGeneric ? constraintArg.GetGenericTypeDefinition() : constraintArg);
            }
        }

        return allGenericParametersMatch ||
            Traverse.Across(parameter, p => p.BaseType!)
                   .Concat(parameter.GetInterfaces())
                   .Any(p => ParameterEqualsConstraint(p, constraint));
    }

#if NET6_0_OR_GREATER
    [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Implementing a real TryMakeGenericType is not worth the effort.")]
#endif
    private static bool ParameterEqualsConstraint(Type parameter, Type constraint)
    {
        var genericArguments = parameter.GenericTypeArguments;
        if (genericArguments.Length > 0 && constraint.IsGenericType)
        {
            var typeDefinition = constraint.GetGenericTypeDefinition();
            if (typeDefinition.GetGenericArguments().Length == genericArguments.Length)
            {
                try
                {
                    var genericType = typeDefinition.MakeGenericType(genericArguments);
                    var constraintArguments = constraint.GenericTypeArguments;

                    for (var i = 0; i < constraintArguments.Length; i++)
                    {
                        var constraintArgument = constraintArguments[i];
                        if (!constraintArgument.IsGenericParameter && !constraintArgument.IsAssignableFrom(genericArguments[i]))
                        {
                            return false;
                        }
                    }

                    return genericType == parameter;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        return false;
    }

    private static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
    {
        return candidateType.GetInterfaces().Concat(
            Traverse.Across(candidateType, t => t.BaseType!));
    }
}
