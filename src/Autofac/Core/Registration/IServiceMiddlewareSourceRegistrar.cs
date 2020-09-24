// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Interface providing fluent syntax for chaining middleware source registrations.
    /// </summary>
    public interface IServiceMiddlewareSourceRegistrar
    {
        /// <summary>
        /// Adds a middleware source to the container.
        /// </summary>
        /// <param name="serviceMiddlewareSource">The middleware source to add.</param>
        /// <returns>
        /// The <see cref="IServiceMiddlewareSourceRegistrar"/> to allow additional chained middleware source registrations.
        /// </returns>
        IServiceMiddlewareSourceRegistrar RegisterServiceMiddlewareSource(IServiceMiddlewareSource serviceMiddlewareSource);
    }
}
