// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac
{
    /// <summary>
    /// Extension methods for registering <see cref="IRegistrationSource"/> instances with a container.
    /// </summary>
    public static class SourceRegistrationExtensions
    {
        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="builder">The builder to register the registration source via.</param>
        /// <param name="registrationSource">The registration source to add.</param>
        /// <returns>
        /// The <see cref="ISourceRegistrar"/> to allow additional chained registration source registrations.
        /// </returns>
        public static ISourceRegistrar RegisterSource(this ContainerBuilder builder, IRegistrationSource registrationSource)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (registrationSource == null)
            {
                throw new ArgumentNullException(nameof(registrationSource));
            }

            var registrar = new SourceRegistrar(builder);
            return registrar.RegisterSource(registrationSource);
        }

        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="builder">The builder to register the registration source with.</param>
        /// <typeparam name="TRegistrationSource">The registration source to add.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="ISourceRegistrar"/> to allow additional chained registration source registrations.
        /// </returns>
        public static ISourceRegistrar RegisterSource<TRegistrationSource>(this ContainerBuilder builder)
            where TRegistrationSource : IRegistrationSource, new()
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.RegisterSource(new TRegistrationSource());
        }

        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="registrar">The source registrar that will make the registration into the container.</param>
        /// <typeparam name="TRegistrationSource">The registration source to add.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="ISourceRegistrar"/> to allow additional chained registration source registrations.
        /// </returns>
        public static ISourceRegistrar RegisterSource<TRegistrationSource>(this ISourceRegistrar registrar)
            where TRegistrationSource : IRegistrationSource, new()
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            return registrar.RegisterSource(new TRegistrationSource());
        }
    }
}
