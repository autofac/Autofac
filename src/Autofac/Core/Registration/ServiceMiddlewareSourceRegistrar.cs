// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Basic implementation of the <see cref="IServiceMiddlewareSourceRegistrar"/>
    /// interface allowing registration of middleware sources into a <see cref="ContainerBuilder"/>
    /// in a fluent format.
    /// </summary>
    internal sealed class ServiceMiddlewareSourceRegistrar : IServiceMiddlewareSourceRegistrar
    {
        /// <summary>
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </summary>
        private readonly ContainerBuilder _builder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceMiddlewareSourceRegistrar"/> class.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ContainerBuilder"/> into which registrations will be made.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        public ServiceMiddlewareSourceRegistrar(ContainerBuilder builder)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <inheritdoc/>
        public IServiceMiddlewareSourceRegistrar RegisterServiceMiddlewareSource(IServiceMiddlewareSource serviceMiddlewareSource)
        {
            if (serviceMiddlewareSource is null)
            {
                throw new ArgumentNullException(nameof(serviceMiddlewareSource));
            }

            _builder.RegisterCallback(cr => cr.AddServiceMiddlewareSource(serviceMiddlewareSource));

            return this;
        }
    }
}
