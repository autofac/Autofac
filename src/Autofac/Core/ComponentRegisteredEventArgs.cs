// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

using Autofac.Core.Registration;

namespace Autofac.Core
{
    /// <summary>
    /// Information about the ocurrence of a component being registered
    /// with a container.
    /// </summary>
    public class ComponentRegisteredEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="IComponentRegistryBuilder" /> into which the registration was made.
        /// </summary>
        public IComponentRegistryBuilder ComponentRegistryBuilder { get; }

        /// <summary>
        /// Gets the component registration.
        /// </summary>
        public IComponentRegistration ComponentRegistration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegisteredEventArgs"/> class.
        /// </summary>
        /// <param name="registryBuilder">The <see cref="IComponentRegistryBuilder" /> into which the registration was made.</param>
        /// <param name="componentRegistration">The component registration.</param>
        public ComponentRegisteredEventArgs(IComponentRegistryBuilder registryBuilder, IComponentRegistration componentRegistration)
        {
            ComponentRegistryBuilder = registryBuilder ?? throw new ArgumentNullException(nameof(registryBuilder));
            ComponentRegistration = componentRegistration ?? throw new ArgumentNullException(nameof(componentRegistration));
        }
    }
}
