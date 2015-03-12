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

namespace Autofac.Core
{
    /// <summary>
    /// Defines a nested structure of lifetimes.
    /// </summary>
    public interface ISharingLifetimeScope : ILifetimeScope
    {
        /// <summary>
        /// The root of the sharing hierarchy.
        /// </summary>
        ISharingLifetimeScope RootLifetimeScope { get; }

        /// <summary>
        /// The parent of this node of the hierarchy, or null.
        /// </summary>
        ISharingLifetimeScope ParentLifetimeScope { get; }

        /// <summary>
        /// Try to retrieve an instance based on a GUID key. If the instance
        /// does not exist, invoke <paramref name="creator"/> to create it.
        /// </summary>
        /// <param name="id">Key to look up.</param>
        /// <param name="creator">Creation function.</param>
        /// <returns>An instance.</returns>
        object GetOrCreateAndShare(Guid id, Func<object> creator);
    }
}
