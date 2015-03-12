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

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// Fired when an instance is looked up.
    /// </summary>
    public class InstanceLookupEndingEventArgs : EventArgs
    {
        readonly IInstanceLookup _instanceLookup;
        readonly bool _newInstanceActivated;

        /// <summary>
        /// Create an instance of the <see cref="InstanceLookupBeginningEventArgs"/> class.
        /// </summary>
        /// <param name="instanceLookup">The instance lookup that is ending.</param>
        /// <param name="newInstanceActivated">True if a new instance was created as part of the operation.</param>
        public InstanceLookupEndingEventArgs(IInstanceLookup instanceLookup, bool newInstanceActivated)
        {
            if (instanceLookup == null) throw new ArgumentNullException("instanceLookup");
            _instanceLookup = instanceLookup;
            _newInstanceActivated = newInstanceActivated;
        }

        /// <summary>
        /// True if a new instance was created as part of the operation.
        /// </summary>
        public bool NewInstanceActivated
        {
            get { return _newInstanceActivated; }
        }

        /// <summary>
        /// The instance lookup operation that is ending.
        /// </summary>
        public IInstanceLookup InstanceLookup
        {
            get { return _instanceLookup; }
        }
    }
}