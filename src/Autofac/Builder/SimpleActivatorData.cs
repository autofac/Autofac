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

using Autofac.Core;
using Autofac.Util;

namespace Autofac.Builder
{
    /// <summary>
    /// An activator builder with no parameters.
    /// </summary>
    public class SimpleActivatorData : IConcreteActivatorData
    {
        readonly IInstanceActivator _activator;

        /// <summary>
        /// Return the provided activator.
        /// </summary>
        /// <param name="activator">The activator to return.</param>
        public SimpleActivatorData(IInstanceActivator activator)
        {
            _activator = Enforce.ArgumentNotNull(activator, "activator");
        }

        /// <summary>
        /// Gets the activator.
        /// </summary>
        public IInstanceActivator Activator
        {
            get
            {
                return _activator;
            }
        }
    }
}
