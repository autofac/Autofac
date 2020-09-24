// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core
{
    /// <summary>
    /// Determines when instances supporting IDisposable are disposed.
    /// </summary>
    public enum InstanceOwnership
    {
        /// <summary>
        /// The lifetime scope does not dispose the instances.
        /// </summary>
        ExternallyOwned,

        /// <summary>
        /// The instances are disposed when the lifetime scope is disposed.
        /// </summary>
        OwnedByLifetimeScope,
    }
}
