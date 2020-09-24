// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core.Lifetime
{
    /// <summary>
    /// Describes when a lifetime scope is ending.
    /// </summary>
    public class LifetimeScopeEndingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeScopeEndingEventArgs"/> class.
        /// </summary>
        /// <param name="lifetimeScope">The lifetime scope that is ending.</param>
        public LifetimeScopeEndingEventArgs(ILifetimeScope lifetimeScope)
        {
            LifetimeScope = lifetimeScope;
        }

        /// <summary>
        /// Gets the lifetime scope that is ending.
        /// </summary>
        public ILifetimeScope LifetimeScope { get; }
    }
}
