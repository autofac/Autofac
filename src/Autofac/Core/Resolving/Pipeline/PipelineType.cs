// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Defines the possible pipeline types.
    /// </summary>
    public enum PipelineType
    {
        /// <summary>
        /// A service pipeline. Usually invokes a <see cref="Registration"/> pipeline when it is finished.
        /// </summary>
        Service,

        /// <summary>
        /// A registration pipeline, used for activating a registration.
        /// </summary>
        Registration
    }
}
