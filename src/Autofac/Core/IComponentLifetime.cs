// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core
{
    /// <summary>
    /// Locates the lifetime to which instances of a component should be attached.
    /// </summary>
    public interface IComponentLifetime
    {
        /// <summary>
        /// Given the most nested scope visible within the resolve operation, find
        /// the scope for the component.
        /// </summary>
        /// <param name="mostNestedVisibleScope">The most nested visible scope.</param>
        /// <returns>The scope for the component.</returns>
        ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope);
    }
}
