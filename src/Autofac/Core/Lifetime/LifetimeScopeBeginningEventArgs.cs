// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core.Lifetime
{
    /// <summary>
    /// Describes when a lifetime scope is beginning.
    /// </summary>
    public class LifetimeScopeBeginningEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LifetimeScopeBeginningEventArgs"/> class.
        /// </summary>
        /// <param name="lifetimeScope">The lifetime scope that is beginning.</param>
        public LifetimeScopeBeginningEventArgs(ILifetimeScope lifetimeScope)
        {
            LifetimeScope = lifetimeScope;
        }

        /// <summary>
        /// Gets the lifetime scope that is beginning.
        /// </summary>
        public ILifetimeScope LifetimeScope { get; }
    }
}
