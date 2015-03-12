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
        /// The name of the property.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Create a <see cref="NamedPropertyParameter"/> with the specified constant value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The property value.</param>
        public NamedPropertyParameter(string name, object value)
            : base(value, pi => {
                PropertyInfo prop;
                return pi.TryGetDeclaringProperty(out prop) &&
                    prop.Name == name;
            })
        {
            Name = Enforce.ArgumentNotNullOrEmpty(name, "name");
        }
    }
}
