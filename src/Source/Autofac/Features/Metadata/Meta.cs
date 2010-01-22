using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Features.Metadata
{
    /// <summary>
    /// Provides metadata with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TMetadata">An interface to which metadata values can be bound.</typeparam>
    public class Meta<T, TMetadata>
    {
        readonly T _value;
        readonly TMetadata _metadata;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="value">The value described by the instance.</param>
        /// <param name="metadata">The metadata describing the value.</param>
        public Meta(T value, TMetadata metadata)
        {
            _value = value;
            _metadata = metadata;
        }

        /// <summary>
        /// The value described by <see cref="Metadata"/>.
        /// </summary>
        public T Value { get { return _value; } }

        /// <summary>
        /// Metadata describing the value.
        /// </summary>
        public TMetadata Metadata { get { return _metadata; } }
    }
}
