// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Autofac.Util
{
    /// <summary>
    /// Provides helper methods for manipulating and inspecting types.
    /// </summary>
    internal static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, bool> IsGenericEnumerableInterfaceCache = new ConcurrentDictionary<Type, bool>();

        private static readonly ConcurrentDictionary<Type, bool> IsGenericListOrCollectionInterfaceTypeCache = new ConcurrentDictionary<Type, bool>();

        private static readonly ConcurrentDictionary<(Type, Type), bool> IsGenericTypeDefinedByCache = new ConcurrentDictionary<(Type, Type), bool>();

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
            return IsGenericEnumerableInterfaceCache.GetOrAdd(
                type, t => type.IsGenericTypeDefinedBy(typeof(IEnumerable<>))
                           || type.IsGenericListOrCollectionInterfaceType());
        }

        /// <summary>
        /// Checks whether a given type is a generic list of colleciton interface type, e.g. <see cref="IList{T}"/>, <see cref="ICollection{T}"/> and the read-only variants.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is one of the supported list/collection types.</returns>
        public static bool IsGenericListOrCollectionInterfaceType(this Type type)
        {
            return IsGenericListOrCollectionInterfaceTypeCache.GetOrAdd(
                type, t => t.IsGenericTypeDefinedBy(typeof(IList<>))
                           || t.IsGenericTypeDefinedBy(typeof(ICollection<>))
                           || t.IsGenericTypeDefinedBy(typeof(IReadOnlyCollection<>))
                           || t.IsGenericTypeDefinedBy(typeof(IReadOnlyList<>)));
        }

        /// <summary>
        /// Checks whether a given type is a closed generic defined by the specifed open generic.
        /// </summary>
        /// <param name="this">The type to check.</param>
        /// <param name="openGeneric">The open generic to check against.</param>
        /// <returns>True if the type is defined by the specified open generic; false otherwise.</returns>
        public static bool IsGenericTypeDefinedBy(this Type @this, Type openGeneric)
        {
            return IsGenericTypeDefinedByCache.GetOrAdd(
                (@this, openGeneric),
                key => !key.Item1.ContainsGenericParameters
                    && key.Item1.IsGenericType
                    && key.Item1.GetGenericTypeDefinition() == key.Item2);
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
                Traverse.Across(parameter, p => p.BaseType)
                       .Concat(parameter.GetInterfaces())
                       .Any(p => ParameterEqualsConstraint(p, constraint));
        }

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Implementing a real TryMakeGenericType is not worth the effort.")]
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
                Traverse.Across(candidateType, t => t.BaseType));
        }
    }
}
