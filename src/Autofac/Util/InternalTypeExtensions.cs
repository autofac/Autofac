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
    public static Type FunctionReturnType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] this Type type)
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
    public static IEnumerable<Type> GetTypesThatClose([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type @this, Type openGeneric)
    {
        return FindAssignableTypesThatClose(@this, openGeneric);
    }

    /// <summary>
    /// Checks whether this type is a closed type of a given generic type.
    /// </summary>
    /// <param name="this">The type we are checking.</param>
    /// <param name="openGeneric">The open generic type to validate against.</param>
    /// <returns>True if <paramref name="this"/> is a closed type of <paramref name="openGeneric"/>. False otherwise.</returns>
    public static bool IsClosedTypeOf([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type @this, Type openGeneric)
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
        => IsCompatibleWithGenericParameterConstraints(genericTypeDefinition.GetGenericArguments(), null, parameters);

    /// <summary>
    /// Determines whether generic parameters are compatible with the specified type parameters (i.e. is it possible
    /// to create a closed generic type from those parameters).
    /// </summary>
    /// <param name="genericArgumentDefinitions">The generic parameters of the generic type definition.</param>
    /// <param name="genericParameterConstraints">
    /// The type constraints of each generic parameter, or null to read them from the generic parameters.
    /// </param>
    /// <param name="parameters">The set of parameters to check against.</param>
    /// <returns>True if the parameters match the generic parameter constraints.</returns>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2062:UnrecognizedReflectionPattern",
        Justification = "The generic argument constraints and candidate parameter types are caller-supplied runtime types from an open generic registration. Their members cannot be statically annotated; preserving them is the responsibility of the consumer that opted into open generic registration (which carries [RequiresUnreferencedCode]/[RequiresDynamicCode]).")]
    public static bool IsCompatibleWithGenericParameterConstraints(Type[] genericArgumentDefinitions, Type[][]? genericParameterConstraints, Type[] parameters)
    {
        for (var i = 0; i < genericArgumentDefinitions.Length; ++i)
        {
            var genericArg = genericArgumentDefinitions[i];
            var parameter = parameters[i];
            var constraints = genericParameterConstraints?[i] ?? genericArg.GetGenericParameterConstraints();

            if (!ParameterCompatibleWithTypeConstraints(parameter, constraints, parameters))
            {
                return false;
            }

            var specialConstraints = genericArg.GenericParameterAttributes;

            if ((specialConstraints & GenericParameterAttributes.DefaultConstructorConstraint)
                != GenericParameterAttributes.None &&
                !parameter.IsValueType && !HasDeclaredPublicParameterlessConstructor(parameter))
            {
                return false;
            }

            if ((specialConstraints & GenericParameterAttributes.ReferenceTypeConstraint)
                != GenericParameterAttributes.None &&
                parameter.IsValueType)
            {
                return false;
            }

            if ((specialConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint)
                != GenericParameterAttributes.None &&
                (!parameter.IsValueType ||
                    (parameter.IsGenericType && IsGenericTypeDefinedBy(parameter, typeof(Nullable<>)))))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether a type is any sort of a collection - array, generic enumerable, or generic list/collection interface.
    /// </summary>
    /// <param name="serviceType">The type to check.</param>
    /// <returns>True if the type is a collection type; false otherwise.</returns>
    public static bool IsCollectionServiceType(this Type serviceType)
    {
        return serviceType.IsArray ||
               serviceType.IsGenericEnumerableInterfaceType();
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
    public static bool IsOpenGenericTypeOf([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type @this, Type type)
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

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2072:UnrecognizedReflectionPattern",
        Justification = "The generic type definition is derived from the base type's own generic instantiation; only its identity is compared (no members are reflected on it beyond the recursive open-generic check). Reachable through open generic / scanning paths whose consumers preserve their types.")]
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

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2072:UnrecognizedReflectionPattern",
        Justification = "Each interface's generic type definition is derived from the interface's own instantiation; only its identity is compared via the recursive open-generic check. Reachable through open generic / scanning paths whose consumers preserve their types.")]
    private static bool CheckInterfacesAreOpenGenericTypeOf([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] this Type @this, Type type)
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
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2067:UnrecognizedReflectionPattern",
        Justification = "The enumerated types are the base types and interfaces of candidateType (whose interfaces are preserved by this method's own annotation). These derived Type values cannot carry static annotations; the interface check on them is best-effort and the closed types involved are preserved by the consumer that opted into the (open generic / scanning) feature.")]
    private static IEnumerable<Type> FindAssignableTypesThatClose([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type candidateType, Type openGenericServiceType)
    {
        return TypesAssignableFrom(candidateType)
            .Where(t => t.IsClosedTypeOf(openGenericServiceType));
    }

    private static bool ParameterCompatibleWithTypeConstraints([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type parameter, Type[] constraints, Type[] parameters)
    {
        foreach (var constraint in constraints)
        {
            if (!ParameterCompatibleWithTypeConstraint(parameter, SubstituteGenericParameterConstraint(parameters, constraint)))
            {
                return false;
            }
        }

        return true;
    }

    private static Type SubstituteGenericParameterConstraint(Type[] parameters, Type constraint)
    {
        if (!constraint.IsGenericParameter)
        {
            return constraint;
        }

        return parameters[constraint.GenericParameterPosition];
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2062:UnrecognizedReflectionPattern",
        Justification = "The generic type arguments and interfaces inspected here come from caller-supplied runtime types in an open generic registration. They cannot carry static annotations; preserving them is the responsibility of the consumer that opted into open generic registration.")]
    private static bool ParameterCompatibleWithTypeConstraint([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type parameter, Type constraint)
    {
        if (constraint.IsAssignableFrom(parameter))
        {
            return true;
        }

        var allGenericParametersMatch = false;
        var baseType = parameter.BaseType ?? parameter;
        if (!constraint.IsInterface && baseType.IsGenericType)
        {
            // GenericTypeArguments returns a new array on every read.
            var baseTypeArguments = baseType.GenericTypeArguments;
            var constraintArguments = constraint.GenericTypeArguments;
            if (baseTypeArguments.Length > 0 && baseTypeArguments.Length == constraintArguments.Length)
            {
                allGenericParametersMatch = AllGenericArgumentsClose(baseTypeArguments, constraintArguments);
            }
        }

        return allGenericParametersMatch || HasTypeEqualToConstraint(parameter, constraint);
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2062:UnrecognizedReflectionPattern",
        Justification = "The generic type arguments inspected here come from caller-supplied runtime types in an open generic registration. They cannot carry static annotations; preserving them is the responsibility of the consumer that opted into open generic registration.")]
    private static bool AllGenericArgumentsClose(Type[] baseTypeArguments, Type[] constraintArguments)
    {
        var allGenericParametersMatch = true;
        for (var i = 0; i < baseTypeArguments.Length; i++)
        {
            var paramArg = baseTypeArguments[i];
            var constraintArg = constraintArguments[i];
            var constraintArgIsGeneric = constraintArg.IsGenericType;

            allGenericParametersMatch &= paramArg.IsClosedTypeOf(constraintArgIsGeneric ? constraintArg.GetGenericTypeDefinition() : constraintArg);
        }

        return allGenericParametersMatch;
    }

    // The same check as looking for a declared public instance constructor without parameters among
    // GetDeclaredPublicConstructors, without building the array of constructors.
    private static bool HasDeclaredPublicParameterlessConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
        => type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null) is not null;

    private static bool HasTypeEqualToConstraint([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type parameter, Type constraint)
    {
        for (var type = parameter; type is not null; type = type.BaseType)
        {
            if (ParameterEqualsConstraint(type, constraint))
            {
                return true;
            }
        }

        foreach (var implementedInterface in parameter.GetInterfaces())
        {
            if (ParameterEqualsConstraint(implementedInterface, constraint))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ParameterEqualsConstraint(Type parameter, Type constraint)
    {
        // Same as closing the constraint's definition over the parameter's generic arguments and comparing
        // the result with the parameter, which can only be equal when both share a definition.
        if (!parameter.IsConstructedGenericType ||
            !constraint.IsGenericType ||
            parameter.GetGenericTypeDefinition() != constraint.GetGenericTypeDefinition())
        {
            return false;
        }

        var genericArguments = parameter.GenericTypeArguments;
        var constraintArguments = constraint.GenericTypeArguments;

        for (var i = 0; i < constraintArguments.Length; i++)
        {
            var constraintArgument = constraintArguments[i];
            if (!constraintArgument.IsGenericParameter && !constraintArgument.IsAssignableFrom(genericArguments[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<Type> TypesAssignableFrom([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type candidateType)
    {
        return candidateType.GetInterfaces().Concat(
            Traverse.Across(candidateType, t => t.BaseType!));
    }
}
