// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// Activator data that can provide an IInstanceActivator instance.
    /// </summary>
    public interface IConcreteActivatorData
    {
        /// <summary>
        /// Gets the instance activator based on the provided data.
        /// </summary>
        IInstanceActivator Activator { get; }
    }
}
