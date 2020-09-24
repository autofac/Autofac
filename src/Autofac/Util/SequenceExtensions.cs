// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Util
{
    /// <summary>
    /// Provides extension methods for working with sequences.
    /// </summary>
    internal static class SequenceExtensions
    {
        /// <summary>
        /// Joins the strings into one single string interspersing the elements with the separator (a-la
        /// System.String.Join()).
        /// </summary>
        /// <param name="elements">The elements.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The joined string.</returns>
        public static string JoinWith(this IEnumerable<string> elements, string separator)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            if (separator == null)
            {
                throw new ArgumentNullException(nameof(separator));
            }

            return string.Join(separator, elements.ToArray());
        }

        /// <summary>
        /// Appends the item to the specified sequence.
        /// </summary>
        /// <typeparam name="T">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="trailingItem">The trailing item.</param>
        /// <returns>The sequence with an item appended to the end.</returns>
        public static IEnumerable<T> AppendItem<T>(this IEnumerable<T> sequence, T trailingItem)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            foreach (var t in sequence)
            {
                yield return t;
            }

            yield return trailingItem;
        }

        /// <summary>
        /// Prepends the item to the specified sequence.
        /// </summary>
        /// <typeparam name="T">The type of element in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="leadingItem">The leading item.</param>
        /// <returns>The sequence with an item prepended.</returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> sequence, T leadingItem)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            yield return leadingItem;

            foreach (var t in sequence)
            {
                yield return t;
            }
        }

        /// <summary>
        /// Add a set of items to the given collection.
        /// </summary>
        /// <typeparam name="T">The set type.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="items">The set of items to add.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
