// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.OpenGenerics;
using Autofac.Features.Scanning;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
    {
        /// <summary>
        /// Register all open generic types in an assembly.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="assemblies">The assemblies from which to register open generic types.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        [RequiresUnreferencedCode(AssemblyScanningWarning)]
        public static IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            RegisterAssemblyOpenGenericTypes(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return ScanningRegistrationExtensions.RegisterOpenGenericAssemblyTypes(builder, assemblies);
        }

        /// <summary>
        /// Specifies how an open generic type from a scanned assembly is mapped to services.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            As<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Service>> serviceMapping)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (serviceMapping == null)
            {
                throw new ArgumentNullException(nameof(serviceMapping));
            }

            return ScanningRegistrationExtensions.As(registration, serviceMapping);
        }

        /// <summary>
        /// Specifies how an open generic type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            As<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Service> serviceMapping)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(t => new[] { serviceMapping(t) });
        }

        /// <summary>
        /// Specifies how an open generic type from a scanned assembly is mapped to a type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            As<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Type> serviceMapping)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(t => new TypedService(serviceMapping(t)));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to types.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            As<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Type>> serviceMapping)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(t => serviceMapping(t).Select(s => (Service)new TypedService(s)));
        }

        /// <summary>
        /// Specifies that an open generic type from a scanned assembly provides its own open generic type as a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            AsSelf<TLimit>(this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(t => t);
        }

        /// <summary>
        /// Filters the scanned open generic types to exclude the provided type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="openGenericType">The open generic type to exclude.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            Except(this IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle> registration, Type openGenericType)
        {
            return registration.Where(t => t != openGenericType);
        }

        /// <summary>
        /// Filters the scanned open generic types to exclude the provided type, providing specific configuration for
        /// the excluded type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="openGenericType">The concrete type to exclude.</param>
        /// <param name="customizedRegistration">Registration for the excepted type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            Except(
                this IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle> registration,
                Type openGenericType,
                Action<IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>> customizedRegistration)
        {
            var result = registration.Except(openGenericType);

            result.ActivatorData.PostScanningCallbacks.Add(cr =>
            {
                var rb = new RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>(
                    new TypedService(openGenericType),
                    new ReflectionActivatorData(openGenericType),
                    new DynamicRegistrationStyle());

                customizedRegistration(rb);

                cr.AddRegistrationSource(new OpenGenericRegistrationSource(rb.RegistrationData, rb.ResolvePipeline, rb.ActivatorData));
            });

            return result;
        }

        /// <summary>
        /// Specifies a subset of open generic types to register from a scanned assembly.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="predicate">Predicate that returns true for types to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            Where<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, bool> predicate)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.ActivatorData.Filters.Add(predicate);
            return registration;
        }

        /// <summary>
        /// Specifies that an open generic type from a scanned assembly is registered if it implements an interface
        /// that opens the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            AsOpenTypesOf<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType)
        {
            return ScanningRegistrationExtensions.AsOpenTypesOf(registration, openGenericServiceType);
        }

        /// <summary>
        /// Specifies that an open generic type from a scanned assembly is registered if it implements an interface
        /// that opens the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            AsOpenTypesOf<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType, object serviceKey)
        {
            return ScanningRegistrationExtensions.AsOpenTypesOf(registration, openGenericServiceType, serviceKey);
        }

        /// <summary>
        /// Specifies that an open generic type from a scanned assembly is registered if it implements an interface
        /// that opens the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            AsOpenTypesOf<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType, Func<Type, object> serviceKeyMapping)
        {
            return ScanningRegistrationExtensions.AsOpenTypesOf(registration, openGenericServiceType, serviceKeyMapping);
        }
    }
}
