﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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
            if (componentRegistry == null) throw new ArgumentNullException(nameof(componentRegistry));
            if (registrationSource == null) throw new ArgumentNullException(nameof(registrationSource));

            ComponentRegistry = componentRegistry;
            RegistrationSource = registrationSource;
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