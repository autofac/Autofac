// This software is part of the Autofac IoC container
// Copyright © 2007 - 2008 Autofac Contributors
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
using Autofac.Core;

namespace Autofac.Features.Indexed
{
    /// <summary>
    /// Provides components by lookup operations via an index (key) type.
    /// </summary>
    /// <typeparam name="TKey">The index key type.</typeparam>
    /// <typeparam name="TValue">The index value type.</typeparam>
    internal class KeyedServiceIndex<TKey, TValue> : IIndex<TKey, TValue>
        where TKey : notnull
    {
        private readonly IComponentContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyedServiceIndex{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="context">The current component context.</param>
        public KeyedServiceIndex(IComponentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public TValue this[TKey key] => (TValue)_context.ResolveService(GetService(key));

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_context.TryResolveService(GetService(key), out var result))
            {
                value = (TValue)result;
                return true;
            }

            value = default!;
            return false;
        }

        private static KeyedService GetService(TKey key)
        {
            return new KeyedService(key, typeof(TValue));
        }
    }
}
