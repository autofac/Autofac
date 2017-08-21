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

namespace Autofac.Core
{
    /// <summary>
    /// Service used as a "flag" to indicate a particular component should be
    /// automatically activated on container build.
    /// </summary>
    internal class AutoActivateService : Service
    {
        /// <summary>
        /// Gets the service description.
        /// </summary>
        /// <value>
        /// Always returns <c>AutoActivate</c>.
        /// </value>
        public override string Description => "AutoActivate";

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>.</param>
        /// <returns>
        /// <see langword="true" /> if the specified <see cref="Object"/> is not <see langword="null" />
        /// and is an <see cref="AutoActivateService"/>; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// All services of this type are considered "equal."
        /// </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            var that = obj as AutoActivateService;
            return that != null;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="System.Object"/>. Always <c>0</c> for this type.
        /// </returns>
        /// <remarks>
        /// <para>
        /// All services of this type are considered "equal" and use the same hash code.
        /// </para>
        /// </remarks>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}
