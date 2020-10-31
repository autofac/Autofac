// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

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
        bool TryGetSharedInstance(Guid id, [NotNullWhen(true)] out object? value);

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
        bool TryGetSharedInstance(Guid primaryId, Guid? qualifyingId, [NotNullWhen(true)] out object? value);

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
