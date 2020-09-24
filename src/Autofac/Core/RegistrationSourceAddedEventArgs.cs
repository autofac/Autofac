// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

using Autofac.Core.Registration;

namespace Autofac.Core
{
    /// <summary>
    /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
    /// </summary>
    public class RegistrationSourceAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationSourceAddedEventArgs"/> class.
        /// </summary>
        /// <param name="componentRegistry">The registry to which the source was added.</param>
        /// <param name="registrationSource">The source that was added.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RegistrationSourceAddedEventArgs(IComponentRegistryBuilder componentRegistry, IRegistrationSource registrationSource)
        {
            ComponentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            RegistrationSource = registrationSource ?? throw new ArgumentNullException(nameof(registrationSource));
        }

        /// <summary>
        /// Gets the registry to which the source was added.
        /// </summary>
        public IRegistrationSource RegistrationSource { get; }

        /// <summary>
        /// Gets the source that was added.
        /// </summary>
        public IComponentRegistryBuilder ComponentRegistry { get; }
    }
}
