// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// An activator builder with no parameters.
    /// </summary>
    public class SimpleActivatorData : IConcreteActivatorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleActivatorData"/> class.
        /// </summary>
        /// <param name="activator">The activator to return.</param>
        public SimpleActivatorData(IInstanceActivator activator)
        {
            Activator = activator ?? throw new ArgumentNullException(nameof(activator));
        }

        /// <summary>
        /// Gets the activator.
        /// </summary>
        public IInstanceActivator Activator { get; }
    }
}
