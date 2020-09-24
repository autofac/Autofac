// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
    {
        /// <summary>
        /// Attaches a predicate to evaluate prior to executing the registration.
        /// The predicate will run at registration time, not runtime, to determine
        /// whether the registration should execute.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="predicate">The predicate to run to determine if the registration should be made.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="predicate" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if <paramref name="registration" /> has no reference to the original callback
        /// with which it was associated (e.g., it wasn't made with a standard registration method
        /// as part of a <see cref="ContainerBuilder"/>).
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            OnlyIf<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, Predicate<IComponentRegistryBuilder> predicate)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var c = registration.RegistrationData.DeferredCallback;
            if (c == null)
            {
                throw new NotSupportedException(RegistrationExtensionsResources.OnlyIfRequiresCallbackContainer);
            }

            var original = c.Callback;
            Action<IComponentRegistryBuilder> updated = registry =>
            {
                if (predicate(registry))
                {
                    original(registry);
                }
            };

            c.Callback = updated;
            return registration;
        }

        /// <summary>
        /// Attaches a predicate such that a registration will only be made if
        /// a specific service type is not already registered.
        /// The predicate will run at registration time, not runtime, to determine
        /// whether the registration should execute.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="serviceType">
        /// The service type to check for to determine if the registration should be made.
        /// Note this is the *service type* - the <c>As&lt;T&gt;</c> part.
        /// </param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="serviceType" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if <paramref name="registration" /> has no reference to the original callback
        /// with which it was associated (e.g., it wasn't made with a standard registration method
        /// as part of a <see cref="ContainerBuilder"/>).
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            IfNotRegistered<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, Type serviceType)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            return registration.OnlyIf(reg => !reg.IsRegistered(new TypedService(serviceType)));
        }
    }
}
