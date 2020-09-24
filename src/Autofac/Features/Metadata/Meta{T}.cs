// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Autofac.Features.Metadata
{
    /// <summary>
    /// Provides a value along with a dictionary of metadata describing the value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class Meta<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Meta{T}"/> class.
        /// </summary>
        /// <param name="value">The value described by the instance.</param>
        /// <param name="metadata">The metadata describing the value.</param>
        public Meta(T value, IDictionary<string, object?> metadata)
        {
            Value = value;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the value described by <see cref="Metadata"/>.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the metadata describing the value.
        /// </summary>
        public IDictionary<string, object?> Metadata { get; }
    }
}
