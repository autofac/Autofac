// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Interface providing fluent syntax for chaining module registrations.
    /// </summary>
    public interface IModuleRegistrar
    {
        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        IModuleRegistrar RegisterModule(IModule module);
    }
}
