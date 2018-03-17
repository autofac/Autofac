// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Autofac.Util
{
    internal static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, bool> IsGenericEnumerableInterfaceCache = new ConcurrentDictionary<Type, bool>();

        private static readonly ConcurrentDictionary<Type, bool> IsGenericListOrCollectionInterfaceTypeCache = new ConcurrentDictionary<Type, bool>();

        private static readonly ConcurrentDictionary<Tuple<Type, Type>, bool> IsGenericTypeDefinedByCache = new ConcurrentDictionary<Tuple<Type, Type>, bool>();

        public static Type FunctionReturnType(this Type type)
        {
            var invoke = type.GetTypeInfo().GetDeclaredMethod("Invoke");
            Enforce.NotNull(invoke);
            return invoke.ReturnType;
        }

        public static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;
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

        public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
        {
            return TypesAssignableFrom(@this).Any(t => t.GetTypeInfo().IsGenericType && !@this.GetTypeInfo().ContainsGenericParameters && t.GetGenericTypeDefinition() == openGeneric);
        }

        public static bool IsCompatibleWithGenericParameterConstraints(this Type genericTypeDefinition, Type[] parameters)
        {
            var genericArgumentDefinitions = genericTypeDefinition.GetTypeInfo().GenericTypeParameters;

            for (var i = 0; i < genericArgumentDefinitions.Length; ++i)
            {
                var argumentDefinitionTypeInfo = genericArgumentDefinitions[i].GetTypeInfo();
                var parameter = parameters[i];
                var parameterTypeInfo = parameter.GetTypeInfo();

                if (argumentDefinitionTypeInfo.GetGenericParameterConstraints()
                    .Select(constraint => SubstituteGenericParameterConstraint(parameters, constraint))
                    .Any(constraint => !ParameterCompatibleWithTypeConstraint(parameter, constraint)))
                {
                    return false;
                }

                var specialConstraints = argumentDefinitionTypeInfo.GenericParameterAttributes;

                if ((specialConstraints & GenericParameterAttributes.DefaultConstructorConstraint)
                    != GenericParameterAttributes.None)
                {
                    if (!parameterTypeInfo.IsValueType && parameterTypeInfo.DeclaredConstructors.All(c => c.GetParameters().Count() != 0))
                    {
                        return false;
                    }
                }

                if ((specialConstraints & GenericParameterAttributes.ReferenceTypeConstraint)
                    != GenericParameterAttributes.None)
                {
                    if (parameterTypeInfo.IsValueType)
                    {
                        return false;
                    }
                }

                if ((specialConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint)
                    != GenericParameterAttributes.None)
                {
                    if (!parameterTypeInfo.IsValueType ||
                        (parameterTypeInfo.IsGenericType && IsGenericTypeDefinedBy(parameter, typeof(Nullable<>))))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsCompilerGenerated(this Type type)
        {
            return type.GetTypeInfo().GetCustomAttributes<CompilerGeneratedAttribute>().Any();
        }

        public static bool IsDelegate(this Type type)
        {
            return type.GetTypeInfo().IsSubclassOf(typeof(Delegate));
        }

        public static bool IsGenericEnumerableInterfaceType(this Type type)
        {
            return IsGenericEnumerableInterfaceCache.GetOrAdd(
                type, t => type.IsGenericTypeDefinedBy(typeof(IEnumerable<>))
                           || type.IsGenericListOrCollectionInterfaceType());
        }

        public static bool IsGenericListOrCollectionInterfaceType(this Type type)
        {
            return IsGenericListOrCollectionInterfaceTypeCache.GetOrAdd(
                type, t => t.IsGenericTypeDefinedBy(typeof(IList<>))
                           || t.IsGenericTypeDefinedBy(typeof(ICollection<>))
                           || t.IsGenericTypeDefinedBy(typeof(IReadOnlyCollection<>))
                           || t.IsGenericTypeDefinedBy(typeof(IReadOnlyList<>)));
        }

        public static bool IsGenericTypeDefinedBy(this Type @this, Type openGeneric)
        {
            return IsGenericTypeDefinedByCache.GetOrAdd(
                Tuple.Create(@this, openGeneric),
                key => !key.Item1.GetTypeInfo().ContainsGenericParameters
                    && key.Item1.GetTypeInfo().IsGenericType
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
            if (!constraint.IsGenericParameter) return constraint;
            return parameters[constraint.GenericParameterPosition];
        }

        private static bool ParameterCompatibleWithTypeConstraint(Type parameter, Type constraint)
        {
            return constraint.GetTypeInfo().IsAssignableFrom(parameter.GetTypeInfo()) ||
                   Traverse.Across(parameter, p => p.GetTypeInfo().BaseType)
                       .Concat(parameter.GetTypeInfo().ImplementedInterfaces)
                       .Any(p => ParameterEqualsConstraint(p, constraint));
        }

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Implementing a real TryMakeGenericType is not worth the effort.")]
        private static bool ParameterEqualsConstraint(Type parameter, Type constraint)
        {
            var genericArguments = parameter.GetTypeInfo().GenericTypeArguments;
            if (genericArguments.Length > 0 && constraint.GetTypeInfo().IsGenericType)
            {
                var typeDefinition = constraint.GetGenericTypeDefinition();
                if (typeDefinition.GetTypeInfo().GenericTypeParameters.Length == genericArguments.Length)
                {
                    try
                    {
                        var genericType = typeDefinition.MakeGenericType(genericArguments);
                        var constraintArguments = constraint.GetTypeInfo().GenericTypeArguments;

                        for (var i = 0; i < constraintArguments.Length; i++)
                        {
                            var constraintArgument = constraintArguments[i].GetTypeInfo();
                            if (!constraintArgument.IsGenericParameter && !constraintArgument.IsAssignableFrom(genericArguments[i].GetTypeInfo()))
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
            return candidateType.GetTypeInfo().ImplementedInterfaces.Concat(
                Traverse.Across(candidateType, t => t.GetTypeInfo().BaseType));
        }
    }
}