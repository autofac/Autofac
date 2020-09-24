// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Basic implementation of the <see cref="IModuleRegistrar"/>
    /// interface allowing registration of modules into a <see cref="ContainerBuilder"/>
    /// in a fluent format.
    /// </summary>
    internal class ModuleRegistrar : IModuleRegistrar
    {
        /// <summary>
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </summary>
        private readonly ContainerBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleRegistrar"/> class.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        public ModuleRegistrar(ContainerBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="module">The module to add.</param>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="module" /> is <see langword="null" />.
        /// </exception>
        public IModuleRegistrar RegisterModule(IModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            _builder.RegisterCallback(module.Configure);
            return this;
        }
    }
}
