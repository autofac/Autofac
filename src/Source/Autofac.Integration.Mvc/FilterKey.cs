// This software is part of the Autofac IoC container
// Copyright © 2012 Autofac Contributors
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
using System.Reflection;
using System.Web.Mvc;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Identifies a filter that is associated with a controller or action.
    /// </summary>
    internal class FilterKey : IEquatable<FilterKey>
    {
        readonly Tuple<Type, FilterScope, MethodInfo> _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterKey"/> class.
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        /// <param name="filterScope">The filter scope.</param>
        /// <param name="methodInfo">The method info.</param>
        public FilterKey(Type controllerType, FilterScope filterScope, MethodInfo methodInfo)
        {
            if (controllerType == null) throw new ArgumentNullException("controllerType");

            _value = new Tuple<Type, FilterScope, MethodInfo>(controllerType, filterScope, methodInfo);
        }

        /// <summary>
        /// Gets the type of the controller.
        /// </summary>
        public Type ControllerType
        {
            get { return _value.Item1; }
        }

        /// <summary>
        /// Gets the filter scope.
        /// </summary>
        public FilterScope FilterScope
        {
            get { return _value.Item2; }
        }

        /// <summary>
        /// Gets the method info.
        /// </summary>
        public MethodInfo MethodInfo
        {
            get { return _value.Item3; }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(FilterKey other)
        {
            return _value.Equals(other._value);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var key = obj as FilterKey;
            return key != null && Equals(key);
        }
    }
}