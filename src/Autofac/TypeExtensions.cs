// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Extends <see cref="Type"/> with methods that are useful in
    /// building scanning rules for <see cref="RegistrationExtensions.RegisterAssemblyTypes"/>.
    /// </summary>
    public static class TypeExtensions
    {
        private const BindingFlags DeclaredOnlyPublicFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private const BindingFlags DeclaredOnlyFlags = DeclaredOnlyPublicFlags | BindingFlags.NonPublic;

        /// <summary>
        /// Returns true if this type is in the <paramref name="namespace"/> namespace
        /// or one of its sub-namespaces.
        /// </summary>
        /// <param name="this">The type to test.</param>
        /// <param name="namespace">The namespace to test.</param>
        /// <returns>True if this type is in the <paramref name="namespace"/> namespace
        /// or one of its sub-namespaces; otherwise, false.</returns>
        public static bool IsInNamespace(this Type @this, string @namespace)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            return @this.Namespace != null &&
                (@this.Namespace == @namespace || @this.Namespace.StartsWith(@namespace + ".", StringComparison.Ordinal));
        }

        /// <summary>
        /// Returns true if this type is in the same namespace as <typeparamref name="T"/>
        /// or one of its sub-namespaces.
        /// </summary>
        /// <param name="this">The type to test.</param>
        /// <returns>True if this type is in the same namespace as <typeparamref name="T"/>
        /// or one of its sub-namespaces; otherwise, false.</returns>
        public static bool IsInNamespaceOf<T>(this Type @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return IsInNamespace(@this, typeof(T).Namespace);
        }

        /// <summary>
        /// Determines whether the candidate type supports any base or
        /// interface that closes the provided generic type.
        /// </summary>
        /// <param name="this">The type to test.</param>
        /// <param name="openGeneric">The open generic against which the type should be tested.</param>
        public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (openGeneric == null)
            {
                throw new ArgumentNullException(nameof(openGeneric));
            }

            if (!openGeneric.IsOpenGeneric())
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, TypeExtensionsResources.NotOpenGenericType, openGeneric.FullName));
            }

            return @this.GetTypesThatClose(openGeneric).Any();
        }

        /// <summary>
        /// Determines whether this type is assignable to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to test assignability to.</typeparam>
        /// <param name="this">The type to test.</param>
        /// <returns>True if this type is assignable to references of type
        /// <typeparamref name="T"/>; otherwise, False.</returns>
        public static bool IsAssignableTo<T>(this Type @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return typeof(T).IsAssignableFrom(@this);
        }

        /// <summary>
        /// Returns an object that represents the specified method declared by the
        /// current type.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>An object that represents the specified method, if found; otherwise, null.</returns>
        public static MethodInfo GetDeclaredMethod(this Type @this, string methodName)
        {
            if (@this is null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            return @this.GetMethod(methodName, DeclaredOnlyFlags);
        }

        /// <summary>
        /// Returns an object that represents the specified property declared by the
        /// current type.
        /// </summary>
        /// <param name="this">The type.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>An object that represents the specified property, if found; otherwise, null.</returns>
        public static PropertyInfo GetDeclaredProperty(this Type @this, string propertyName)
        {
            if (@this is null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (propertyName is null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            return @this.GetProperty(propertyName, DeclaredOnlyFlags);
        }

        /// <summary>
        /// Returns a collection of constructor infomration that represents the declared constructors
        /// for the type (public and private).
        /// </summary>
        /// <param name="this">The type.</param>
        /// <returns>A collection of constructors.</returns>
        public static ConstructorInfo[] GetDeclaredConstructors(this Type @this)
        {
            if (@this is null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.GetConstructors(DeclaredOnlyFlags);
        }

        /// <summary>
        /// Returns a collection of constructor infomration that represents the declared constructors
        /// for the type (public only).
        /// </summary>
        /// <param name="this">The type.</param>
        /// <returns>A collection of constructors.</returns>
        public static ConstructorInfo[] GetDeclaredPublicConstructors(this Type @this)
        {
            if (@this is null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.GetConstructors(DeclaredOnlyPublicFlags);
        }

        /// <summary>
        /// Finds a constructor with the matching type parameters.
        /// </summary>
        /// <param name="type">The type being tested.</param>
        /// <param name="constructorParameterTypes">The types of the contractor to find.</param>
        /// <returns>The <see cref="ConstructorInfo"/> is a match is found; otherwise, <c>null</c>.</returns>
        public static ConstructorInfo GetMatchingConstructor(this Type type, Type[] constructorParameterTypes)
        {
            return type.GetDeclaredConstructors().FirstOrDefault(
                c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(constructorParameterTypes));
        }
    }
}
