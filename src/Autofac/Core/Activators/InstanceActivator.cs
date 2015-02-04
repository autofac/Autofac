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

using System;
using Autofac.Util;

namespace Autofac.Core.Activators
{
    /// <summary>
    /// Base class for instance activators.
    /// </summary>
    public abstract class InstanceActivator : Disposable
    {
        readonly Type _limitType;

        /// <summary>
        /// Create an instance activator that will return instances compatible
        /// with <paramref name="limitType"/>.
        /// </summary>
        /// <param name="limitType">Most derived type to which instances can be cast.</param>
        protected InstanceActivator(Type limitType)
        {
            _limitType = Enforce.ArgumentNotNull(limitType, "limitType");
        }

        /// <summary>
        /// The most specific type that the component instances are known to be castable to.
        /// </summary>
        public Type LimitType
        {
            get { return _limitType; }
        }

        /// <summary>
        /// Gets a string representation of the activator.
        /// </summary>
        /// <returns>A string describing the activator.</returns>
        public override string ToString()
        {
            return LimitType.Name + " (" + GetType().Name + ")";
        }
    }
}
