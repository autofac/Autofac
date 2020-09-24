// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Interface providing fluent syntax for chaining registration source registrations.
    /// </summary>
    public interface ISourceRegistrar
    {
        /// <summary>
        /// Add a registration source to the container.
        /// </summary>
        /// <param name="registrationSource">The registration source to add.</param>
        /// <returns>
        /// The <see cref="ISourceRegistrar"/> to allow additional chained registration source registrations.
        /// </returns>
        ISourceRegistrar RegisterSource(IRegistrationSource registrationSource);
    }
}
