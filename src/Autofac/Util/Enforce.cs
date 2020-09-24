// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            if (value.Any(e => e == null))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.ElementCannotBeNull, name));
            }

            return value;
        }

        /// <summary>
        /// Enforces that the provided object is non-null.
        /// </summary>
        /// <typeparam name="T">The type of value being checked.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns><paramref name="value"/> if not null.</returns>
        public static T NotNull<T>([ValidatedNotNull]T value)
            where T : class
        {
            if (value == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, EnforceResources.CannotBeNull, typeof(T).FullName));
            }

            return value;
        }

        /// <summary>
        /// Enforce that an argument is not null or empty. Returns the
        /// value if valid so that it can be used inline in
        /// base initialiser syntax.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="description">The description.</param>
        /// <returns><paramref name="value"/>if not null or empty.</returns>
        public static string ArgumentNotNullOrEmpty([ValidatedNotNull]string value, string description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (value == null)
            {
                throw new ArgumentNullException(description);
            }

            if (value.Length == 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.CannotBeEmpty, description));
            }

            return value;
        }

        /// <summary>
        /// Enforce that the argument is a delegate type.
        /// </summary>
        /// <param name="delegateType">The type to test.</param>
        public static void ArgumentTypeIsFunction(Type delegateType)
        {
            if (delegateType == null)
            {
                throw new ArgumentNullException(nameof(delegateType));
            }

            MethodInfo invoke = delegateType.GetDeclaredMethod("Invoke");

            if (invoke == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.NotDelegate, delegateType));
            }

            if (invoke.ReturnType == typeof(void))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, EnforceResources.DelegateReturnsVoid, delegateType));
            }
        }
    }
}
