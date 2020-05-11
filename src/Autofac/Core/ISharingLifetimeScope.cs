// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
        /// Gets the root of the sharing hierarchy.
        /// </summary>
        ISharingLifetimeScope RootLifetimeScope { get; }

        /// <summary>
        /// Gets the parent of this node of the hierarchy, or null.
        /// </summary>
        ISharingLifetimeScope? ParentLifetimeScope { get; }

        /// <summary>
        /// Try to retrieve a shared instance based on a GUID key.
        /// </summary>
        /// <param name="id">Key to look up.</param>
        /// <param name="value">The instance that has the specified key.</param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        bool TryGetSharedInstance(Guid id, out object value);

        /// <summary>
        /// Try to retrieve a shared instance based on a primary GUID key and
        /// possible secondary qualifying GUID key.
        /// </summary>
        /// <param name="primaryId">Key to look up.</param>
        /// <param name="qualifyingId">
        /// Secondary key to look up, to better identify an instance that wraps around another instance
        /// or is otherwise "namespaced" by it.
        /// </param>
        /// <param name="value">The instance that has the specified keys.</param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        bool TryGetSharedInstance(Guid primaryId, Guid? qualifyingId, out object value);

        /// <summary>
        /// Creates a shared instance with a GUID key.
        /// </summary>
        /// <param name="id">Key.</param>
        /// <param name="creator">A function that will create the instance when called.</param>
        /// <returns>The shared instance.</returns>
        object CreateSharedInstance(Guid id, Func<object> creator);

        /// <summary>
        /// Creates a shared instance with a primary GUID key and
        /// possible secondary qualifying GUID key.
        /// </summary>
        /// <param name="primaryId">Key.</param>
        /// <param name="qualifyingId">
        /// Secondary key, to better identify an instance that wraps around another instance
        /// or is otherwise "namespaced" by it.
        /// </param>
        /// <param name="creator">A function that will create the instance when called.</param>
        /// <returns>The shared instance.</returns>
        object CreateSharedInstance(Guid primaryId, Guid? qualifyingId, Func<object> creator);
    }
}
