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
using System.Linq;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Extends <see cref="System.Type"/> with methods that are useful in
    /// building scanning rules for <see cref="RegistrationExtensions.RegisterAssemblyTypes"/>.
    /// </summary>
    public static class TypeExtensions
    {
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
            if (@this == null) throw new ArgumentNullException("this");
            if (@namespace == null) throw new ArgumentNullException("namespace");
            return @this.Namespace != null &&
                (@this.Namespace == @namespace || @this.Namespace.StartsWith(@namespace + "."));
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
            if (@this == null) throw new ArgumentNullException("this");
            return IsInNamespace(@this, typeof (T).Namespace);
        }

        /// <summary>Determines whether the candidate type supports any base or 
        /// interface that closes the provided generic type.</summary>
        /// <param name="this"></param>
        /// <param name="openGeneric"></param>
        /// <returns></returns>
        public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
        {
            if (@this == null) throw new ArgumentNullException("this");
            if (openGeneric == null) throw new ArgumentNullException("openGeneric");

            if (!(openGeneric.IsGenericTypeDefinition || openGeneric.ContainsGenericParameters))
                throw new ArgumentException(string.Format(TypeExtensionsResources.NotOpenGenericType, openGeneric.FullName));

            return @this.GetTypesThatClose(openGeneric).Any();
        }

        /// <summary>
        /// Determines whether this type is assignable to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to test assignability to.</typeparam>
        /// <returns>True if this type is assignable to references of type
        /// <typeparamref name="T"/>; otherwise, False.</returns>
        public static bool IsAssignableTo<T>(this Type @this)
        {
            if (@this == null) throw new ArgumentNullException("this");
            return typeof (T).IsAssignableFrom(@this);
        }

    }
}
