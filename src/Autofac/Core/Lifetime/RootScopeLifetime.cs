// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core.Lifetime
{
    /// <summary>
    /// Attaches the component's lifetime to the root scope.
    /// </summary>
    public class RootScopeLifetime : IComponentLifetime
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="RootScopeLifetime"/> behaviour.
        /// </summary>
        public static IComponentLifetime Instance { get; } = new RootScopeLifetime();

        /// <summary>
        /// Given the most nested scope visible within the resolve operation, find
        /// the scope for the component.
        /// </summary>
        /// <param name="mostNestedVisibleScope">The most nested visible scope.</param>
        /// <returns>The scope for the component.</returns>
        public ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope)
        {
            if (mostNestedVisibleScope == null)
            {
                throw new ArgumentNullException(nameof(mostNestedVisibleScope));
            }

            return mostNestedVisibleScope.RootLifetimeScope;
        }
    }
}
