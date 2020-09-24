// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Autofac.Util
{
    /// <summary>
    /// Provides a method to support traversing structures.
    /// </summary>
    internal static class Traverse
    {
        /// <summary>
        /// Traverse across a set, taking the first item in the set, and a function to determine the next item.
        /// </summary>
        /// <typeparam name="T">The set type.</typeparam>
        /// <param name="first">The first item in the set.</param>
        /// <param name="next">A callback that will take the current item in the set, and output the next one.</param>
        /// <returns>An enumerable of the set.</returns>
        public static IEnumerable<T> Across<T>(T first, Func<T, T> next)
            where T : class
        {
            var item = first;
            while (item != null)
            {
                yield return item;
                item = next(item);
            }
        }
    }
}
