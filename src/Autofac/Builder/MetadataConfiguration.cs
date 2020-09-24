// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Builder
{
    /// <summary>
    /// Used with the WithMetadata configuration method to
    /// associate key-value pairs with an <see cref="IComponentRegistration"/>.
    /// </summary>
    /// <typeparam name="TMetadata">Interface with properties whose names correspond to
    /// the property keys.</typeparam>
    /// <remarks>This feature was suggested by OJ Reeves (@TheColonial).</remarks>
    public class MetadataConfiguration<TMetadata>
    {
        private readonly IDictionary<string, object?> _properties = new Dictionary<string, object?>();

        /// <summary>
        /// Gets the set of properties that have been provided.
        /// </summary>
        internal IEnumerable<KeyValuePair<string, object?>> Properties => _properties;

        /// <summary>
        /// Set one of the property values.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyAccessor">An expression that accesses the property to set.</param>
        /// <param name="value">The property value to set.</param>
        public MetadataConfiguration<TMetadata> For<TProperty>(Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value)
        {
            if (propertyAccessor == null)
            {
                throw new ArgumentNullException(nameof(propertyAccessor));
            }

            var pn = ReflectionExtensions.GetProperty(propertyAccessor).Name;
            _properties.Add(pn, value);
            return this;
        }
    }
}
