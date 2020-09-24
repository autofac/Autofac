// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Util;

namespace Autofac.Core
{
    /// <summary>
    /// A property identified by name. When applied to a reflection-based
    /// component, the name will be matched against property names.
    /// </summary>
    public class NamedPropertyParameter : ConstantParameter
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPropertyParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The property value.</param>
        public NamedPropertyParameter(string name, object value)
            : base(value, pi =>
            {
                return pi.TryGetDeclaringProperty(out PropertyInfo? prop) &&
                    prop.Name == name;
            })
        {
            Name = Enforce.ArgumentNotNullOrEmpty(name, "name");
        }
    }
}
