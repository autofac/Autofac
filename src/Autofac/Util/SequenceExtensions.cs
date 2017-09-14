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
using System.Linq;

namespace Autofac.Util
{
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
            if (elements == null) throw new ArgumentNullException(nameof(elements));
            if (separator == null) throw new ArgumentNullException(nameof(separator));

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
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));

            foreach (var t in sequence)
                yield return t;

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
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));

            yield return leadingItem;

            foreach (var t in sequence)
                yield return t;
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }
    }
}
