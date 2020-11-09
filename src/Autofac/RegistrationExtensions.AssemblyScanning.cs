// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
    {
        private const string AssemblyScanningWarning = "Assembly scanning is unlikely to be compatible with member-level trimming; the linker will not be able to determine which types to preserve.";

        /// <summary>
        /// Register all types in an assembly.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="assemblies">The assemblies from which to register types.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        [RequiresUnreferencedCode(AssemblyScanningWarning)]
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterAssemblyTypes(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            return ScanningRegistrationExtensions.RegisterAssemblyTypes(builder, assemblies);
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Service>> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
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
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Service> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(t => new[] { serviceMapping(t) });
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, Type> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(t => new TypedService(serviceMapping(t)));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceMapping">Function mapping types to services.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            As<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<Type>> serviceMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(t => serviceMapping(t).Select(s => (Service)new TypedService(s)));
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly provides its own concrete type as a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle>
            AsSelf<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(t => t);
        }

        /// <summary>
        /// Specifies that a type provides its own concrete type as a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TConcreteActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle>
            AsSelf<TLimit, TConcreteActivatorData>(this IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> registration)
            where TConcreteActivatorData : IConcreteActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(registration.ActivatorData.Activator.LimitType);
        }

        /// <summary>
        /// Specifies that a type provides its own concrete type as a service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle>
            AsSelf<TLimit>(this IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(registration.ActivatorData.ImplementationType);
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered as providing all of its
        /// implemented interfaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle>
            AsImplementedInterfaces<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(t => GetImplementedInterfaces(t));
        }

        /// <summary>
        /// Specifies that a type is registered as providing all of its implemented interfaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TConcreteActivatorData">Activator data type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle>
            AsImplementedInterfaces<TLimit, TConcreteActivatorData>(this IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> registration)
            where TConcreteActivatorData : IConcreteActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.As(GetImplementedInterfaces(registration.ActivatorData.Activator.LimitType));
        }

        /// <summary>
        /// Specifies that a type is registered as providing all of its implemented interfaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle>
            AsImplementedInterfaces<TLimit>(this IRegistrationBuilder<TLimit, ReflectionActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            var implementationType = registration.ActivatorData.ImplementationType;
            return registration.As(GetImplementedInterfaces(implementationType));
        }

        private static Type[] GetImplementedInterfaces(Type type)
        {
            var interfaces = type.GetInterfaces().Where(i => i != typeof(IDisposable));
            return type.IsInterface ? interfaces.AppendItem(type).ToArray() : interfaces.ToArray();
        }

        /// <summary>
        /// Specifies that the components being registered should only be made the default for services
        /// that have not already been registered.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
            PreserveExistingDefaults<TLimit, TRegistrationStyle>(this
            IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration)
        {
            return ScanningRegistrationExtensions.PreserveExistingDefaults(registration);
        }

        /// <summary>
        /// Register the types in a list.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="types">The types to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterTypes(this ContainerBuilder builder, params Type[] types)
        {
            return ScanningRegistrationExtensions.RegisterTypes(builder, types);
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered if it implements an interface
        /// that closes the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AsClosedTypesOf(registration, openGenericServiceType);
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered if it implements an interface
        /// that closes the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType, object serviceKey)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AsClosedTypesOf(registration, openGenericServiceType, serviceKey);
        }

        /// <summary>
        /// Specifies that a type from a scanned assembly is registered if it implements an interface
        /// that closes the provided open generic interface type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType, Func<Type, object> serviceKeyMapping)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AsClosedTypesOf(registration, openGenericServiceType, serviceKeyMapping);
        }

        /// <summary>
        /// Filters the scanned types to include only those assignable to the provided
        /// type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="type">The type or interface which all classes must be assignable from.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AssignableTo<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Type type)
            where TScanningActivatorData : ScanningActivatorData
        {
            return ScanningRegistrationExtensions.AssignableTo(registration, type);
        }

        /// <summary>
        /// Filters the scanned types to include only those assignable to the provided
        /// type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">The type or interface which all classes must be assignable from.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            AssignableTo<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.AssignableTo(typeof(T));
        }

        /// <summary>
        /// Filters the scanned types to exclude the provided type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">The concrete type to exclude.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Except<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.Where(t => t != typeof(T));
        }

        /// <summary>
        /// Filters the scanned types to exclude the provided type, providing specific configuration for
        /// the excluded type.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="customizedRegistration">Registration for the excepted type.</param>
        /// <typeparam name="T">The concrete type to exclude.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Except<T>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration,
                Action<IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>> customizedRegistration)
            where T : notnull
        {
            var result = registration.Except<T>();

            result.ActivatorData.PostScanningCallbacks.Add(cr =>
            {
                var rb = RegistrationBuilder.ForType<T>();
                customizedRegistration(rb);
                RegistrationBuilder.RegisterSingleComponent(cr, rb);
            });

            return result;
        }

        /// <summary>
        /// Filters the scanned types to include only those in the namespace of the provided type
        /// or one of its sub-namespaces.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">A type in the target namespace.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            InNamespaceOf<T>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            // Namespace is always non-null for concrete type parameters.
            return registration.InNamespace(typeof(T).Namespace!);
        }

        /// <summary>
        /// Filters the scanned types to include only those in the provided namespace
        /// or one of its sub-namespaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="ns">The namespace from which types will be selected.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            InNamespace<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                string ns)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (ns == null)
            {
                throw new ArgumentNullException(nameof(ns));
            }

            return registration.Where(t => t.IsInNamespace(ns));
        }

        /// <summary>
        /// Specifies a subset of types to register from a scanned assembly.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="predicate">Predicate that returns true for types to register.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Where<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, bool> predicate)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.ActivatorData.Filters.Add(predicate);
            return registration;
        }
    }
}
