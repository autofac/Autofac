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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Autofac.Util
{
    static class TypeExtensions
    {
        public static readonly Type[] EmptyTypes = new Type[0];

        static readonly Type ReadOnlyCollectionType = Type.GetType("System.Collections.Generic.IReadOnlyCollection`1", false);

        static readonly Type ReadOnlyListType = Type.GetType("System.Collections.Generic.IReadOnlyList`1", false);

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
        /// Looks for an interface on the candidate type that closes the provided open generic interface type.
        /// </summary>
        /// <param name="candidateType">The type that is being checked for the interface.</param>
        /// <param name="openGenericServiceType">The open generic service type to locate.</param>
        /// <returns>True if a closed implementation was found; otherwise false.</returns>
        static IEnumerable<Type> FindAssignableTypesThatClose(Type candidateType, Type openGenericServiceType)
        {
            return TypesAssignableFrom(candidateType)
                .Where(t => IsGenericTypeDefinedBy(t, openGenericServiceType));
        }

        static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
        {
            return candidateType.GetInterfaces().Concat(
                Traverse.Across(candidateType, t => t.BaseType));
        }

        public static bool IsGenericTypeDefinedBy(this Type @this, Type openGeneric)
        {
            if (@this == null) throw new ArgumentNullException("this");
            if (openGeneric == null) throw new ArgumentNullException("openGeneric");

            return !@this.ContainsGenericParameters && @this.IsGenericType && @this.GetGenericTypeDefinition() == openGeneric;
        }

        public static bool IsDelegate(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return type.IsSubclassOf(typeof(Delegate));
        }

        public static Type FunctionReturnType(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            var invoke = type.GetMethod("Invoke");
            Enforce.NotNull(invoke);
            return invoke.ReturnType;
        }

        public static bool IsCompatibleWithGenericParameterConstraints(this Type genericTypeDefinition, Type[] parameters)
        {
            var genericArgumentDefinitions = genericTypeDefinition.GetGenericArguments();

            for (var i = 0; i < genericArgumentDefinitions.Length; ++i)
            {
                var argumentDefinition = genericArgumentDefinitions[i];
                var parameter = parameters[i];

                if (argumentDefinition.GetGenericParameterConstraints()
                    .Any(constraint => !ParameterCompatibleWithTypeConstraint(parameter, constraint)))
                {
                    return false;
                }

                var specialConstraints = argumentDefinition.GenericParameterAttributes;

                if ((specialConstraints & GenericParameterAttributes.DefaultConstructorConstraint)
                    != GenericParameterAttributes.None)
                {
                    if (!parameter.IsValueType && parameter.GetConstructor(EmptyTypes) == null)
                        return false;
                }

                if ((specialConstraints & GenericParameterAttributes.ReferenceTypeConstraint)
                    != GenericParameterAttributes.None)
                {
                    if (parameter.IsValueType)
                        return false;
                }

                if ((specialConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint)
                    != GenericParameterAttributes.None)
                {
                    if (!parameter.IsValueType ||
                        (parameter.IsGenericType && IsGenericTypeDefinedBy(parameter, typeof(Nullable<>))))
                        return false;
                }
            }

            return true;
        }

        static bool ParameterCompatibleWithTypeConstraint(Type parameter, Type constraint)
        {
            return constraint.IsAssignableFrom(parameter) ||
                   Traverse.Across(parameter, p => p.BaseType)
                       .Concat(parameter.GetInterfaces())
                       .Any(p => ParameterEqualsConstraint(p, constraint));
        }

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Implementing a real TryMakeGenericType is not worth the effort.")]
        static bool ParameterEqualsConstraint(Type parameter, Type constraint)
        {
            var genericArguments = parameter.GetGenericArguments();
            if (genericArguments.Length > 0 && constraint.IsGenericType)
            {
                var typeDefinition = constraint.GetGenericTypeDefinition();
                if (typeDefinition.GetGenericArguments().Length == genericArguments.Length)
                {
                    try
                    {
                        var genericType = typeDefinition.MakeGenericType(genericArguments);
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

        public static bool IsCompatibleWith(this Type type, Type that)
        {
            // This previously used Type.GUID which is not available in PCL.
            // Leaving this method here for now because it is public and could have been used by others.
            return type.Equals(that);
        }

        public static int GetCompatibleHashCode(this Type type)
        {
            return type.GetHashCode();
        }

        public static bool IsGenericEnumerableInterfaceType(this Type type)
        {
            return (type.IsGenericTypeDefinedBy(typeof(IEnumerable<>))
                || type.IsGenericListOrCollectionInterfaceType());
        }

        public static bool IsGenericListOrCollectionInterfaceType(this Type type)
        {
            return type.IsGenericTypeDefinedBy(typeof(IList<>))
                   || type.IsGenericTypeDefinedBy(typeof(ICollection<>))
                   || (ReadOnlyCollectionType != null && type.IsGenericTypeDefinedBy(ReadOnlyCollectionType))
                   || (ReadOnlyListType != null && type.IsGenericTypeDefinedBy(ReadOnlyListType));
        }
    }
}

