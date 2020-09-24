// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.GeneratedFactories
{
    /// <summary>
    /// Helper methods for registering factories.
    /// </summary>
    internal static class GeneratedFactoryRegistrationExtensions
    {
        /// <summary>
        /// Registers a factory delegate.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegateType">Factory type to generate.</param>
        /// <param name="service">The service that the delegate will return instances of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>Factory delegates are provided automatically in Autofac 2, and
        /// this method is generally not required.</remarks>
        internal static IRegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory<TLimit>(ContainerBuilder builder, Type delegateType, Service service)
            where TLimit : notnull
        {
            var activatorData = new GeneratedFactoryActivatorData(delegateType, service);

            var rb = new RegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle>(
                new TypedService(delegateType),
                activatorData,
                new SingleRegistrationStyle());

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb.InstancePerLifetimeScope();
        }
    }
}
