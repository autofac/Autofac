// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Basic implementation of the <see cref="ISourceRegistrar"/>
    /// interface allowing registration of registration sources into a <see cref="ContainerBuilder"/>
    /// in a fluent format.
    /// </summary>
    internal sealed class SourceRegistrar : ISourceRegistrar
    {
        /// <summary>
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </summary>
        private readonly ContainerBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceRegistrar"/> class.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        public SourceRegistrar(ContainerBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="registrationSource">The registration source to add.</param>
        /// <returns>
        /// The <see cref="ISourceRegistrar"/> to allow additional chained registration source registrations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="registrationSource" /> is <see langword="null" />.
        /// </exception>
        public ISourceRegistrar RegisterSource(IRegistrationSource registrationSource)
        {
            if (registrationSource == null)
            {
                throw new ArgumentNullException(nameof(registrationSource));
            }

            _builder.RegisterCallback(cr => cr.AddRegistrationSource(registrationSource));
            return this;
        }
    }
}
