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
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Autofac.Util
{
    /// <summary>
    /// Helper methods used throughout the codebase.
    /// </summary>
    internal static class Enforce
    {
        /// <summary>
        /// Enforce that sequence does not contain null. Returns the
        /// value if valid so that it can be used inline in
        /// base initialiser syntax.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The parameter name.</param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IEnumerable<T> ArgumentElementNotNull<T>(IEnumerable<T> value, string name)
            where T : class
        {
            if (value == null) throw new ArgumentNullException(name);

            if (value.Any(e => e == null))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.ElementCannotBeNull, name));

            return value;
        }

        /// <summary>
        /// Enforces that the provided object is non-null.
        /// </summary>
        /// <typeparam name="T">The type of value being checked.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns><paramref name="value"/></returns>
        public static T NotNull<T>([ValidatedNotNull]T value)
            where T : class
        {
            if (value == null)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, EnforceResources.CannotBeNull, typeof(T).FullName));

            return value;
        }

        /// <summary>
        /// Enforce that an argument is not null or empty. Returns the
        /// value if valid so that it can be used inline in
        /// base initialiser syntax.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="description">The description.</param>
        /// <returns><paramref name="value"/></returns>
        public static string ArgumentNotNullOrEmpty([ValidatedNotNull]string value, string description)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (value == null) throw new ArgumentNullException(description);

            if (value.Length == 0)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.CannotBeEmpty, description));

            return value;
        }

        /// <summary>
        /// Enforce that the argument is a delegate type.
        /// </summary>
        /// <param name="delegateType">The type to test.</param>
        public static void ArgumentTypeIsFunction(Type delegateType)
        {
            if (delegateType == null) throw new ArgumentNullException(nameof(delegateType));

            MethodInfo invoke = delegateType.GetTypeInfo().GetDeclaredMethod("Invoke");

            if (invoke == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.NotDelegate, delegateType));

            if (invoke.ReturnType == typeof(void))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.DelegateReturnsVoid, delegateType));
        }
    }
}
