// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// Extension methods for controlling and accessing registration order.
    /// </summary>
    internal static class RegistrationOrderExtensions
    {
        /// <summary>
        /// Gets the registration order value from the registration.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>The original registration order value.</returns>
        internal static long GetRegistrationOrder(this IComponentRegistration registration)
        {
            return registration.Metadata.TryGetValue(MetadataKeys.RegistrationOrderMetadataKey, out object? value) ? (long)value! : long.MaxValue;
        }

        /// <summary>
        /// Indicates that a registration should inherit its registration order from another registration.
        /// </summary>
        /// <typeparam name="TLimit">The limit type.</typeparam>
        /// <typeparam name="TActivatorData">The activator data type.</typeparam>
        /// <typeparam name="TSingleRegistrationStyle">The registration style type.</typeparam>
        /// <param name="registration">The registration builder.</param>
        /// <param name="source">The source registration to take the order from.</param>
        /// <returns>The registration builder.</returns>
        internal static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> InheritRegistrationOrderFrom<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                IComponentRegistration source)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            var sourceRegistrationOrder = source.GetRegistrationOrder();
            registration.RegistrationData.Metadata[MetadataKeys.RegistrationOrderMetadataKey] = sourceRegistrationOrder;

            return registration;
        }
    }
}
