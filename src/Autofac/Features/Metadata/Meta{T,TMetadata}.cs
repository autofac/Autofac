// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Features.Metadata
{
    /// <summary>
    /// Provides a value along with metadata describing the value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TMetadata">An interface to which metadata values can be bound.</typeparam>
    public class Meta<T, TMetadata>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Meta{T,TMetadata}"/> class.
        /// </summary>
        /// <param name="value">The value described by the instance.</param>
        /// <param name="metadata">The metadata describing the value.</param>
        public Meta(T value, TMetadata metadata)
        {
            Value = value;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the value described by <see cref="Metadata"/>.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets metadata describing the value.
        /// </summary>
        public TMetadata Metadata { get; }
    }
}
