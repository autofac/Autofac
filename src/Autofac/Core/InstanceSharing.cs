// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core
{
    /// <summary>
    /// Determines whether instances are shared within a lifetime scope.
    /// </summary>
    public enum InstanceSharing
    {
        /// <summary>
        /// Each request for an instance will return a new object.
        /// </summary>
        None,

        /// <summary>
        /// Each request for an instance will return the same object.
        /// </summary>
        Shared,
    }
}
