// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
