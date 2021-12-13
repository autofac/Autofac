// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        /// Filters the scanned types to include only those assignable to the provided.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The type or interface which all classes must be assignable from.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            AssignableTo<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType)
        {
            return ScanningRegistrationExtensions.AssignableTo(registration, openGenericServiceType);
        }

        /// <summary>
        /// Filters the scanned types to include only those assignable to the provided.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The type or interface which all classes must be assignable from.</param>
        /// <param name="serviceKey">Key of the service.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            AssignableTo<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType, object serviceKey)
        {
            return ScanningRegistrationExtensions.AssignableTo(registration, openGenericServiceType, serviceKey);
        }

        /// <summary>
        /// Filters the scanned types to include only those assignable to the provided.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="openGenericServiceType">The type or interface which all classes must be assignable from.</param>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            AssignableTo<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration, Type openGenericServiceType, Func<Type, object> serviceKeyMapping)
        {
            return ScanningRegistrationExtensions.AssignableTo(registration, openGenericServiceType, serviceKeyMapping);
        }

        /// <summary>
        /// Filters the scanned open generic types to include only those in the namespace of the provided type
        /// or one of its sub-namespaces.
        /// </summary>
        /// <param name="registration">Registration to filter types from.</param>
        /// <typeparam name="T">A type in the target namespace.</typeparam>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            InNamespaceOf<T>(this IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            // Namespace is always non-null for concrete type parameters.
            return registration.InNamespace(typeof(T).Namespace!);
        }

        /// <summary>
        /// Filters the scanned types to include only those in the provided namespace
        /// or one of its sub-namespaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <param name="ns">The namespace from which types will be selected.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            InNamespace<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
                string ns)
        {
            if (string.IsNullOrEmpty(ns))
            {
                throw new ArgumentNullException(nameof(ns));
            }

            return registration.Where(t => t.IsInNamespace(ns));
        }

        /// <summary>
        /// Specifies that an open generic type from a scanned assembly is registered as providing all of its
        /// implemented interfaces.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            AsImplementedInterfaces<TLimit>(this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.As(t => t.GetOpenGenericImplementedInterfaces());
        }

        /// <summary>
        /// Specify how a type from a scanned assembly provides metadata.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set metadata on.</param>
        /// <param name="metadataMapping">A function mapping the type to a list of metadata items.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
            WithMetadata<TLimit, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, IEnumerable<KeyValuePair<string, object?>>> metadataMapping)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.WithMetadata(metadataMapping(t)));
            return registration;
        }

        /// <summary>
        /// Use the properties of an attribute (or interface implemented by an attribute) on the scanned type
        /// to provide metadata values.
        /// </summary>
        /// <remarks>Inherited attributes are supported; however, there must be at most one matching attribute
        /// in the inheritance chain.</remarks>
        /// <typeparam name="TAttribute">The attribute applied to the scanned type.</typeparam>
        /// <param name="registration">Registration to set metadata on.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
            WithMetadataFrom<TAttribute>(
                this IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            var attrType = typeof(TAttribute);
            var metadataProperties = attrType
                .GetRuntimeProperties()
                .Where(pi => pi.CanRead);

            return registration.WithMetadata(t =>
            {
                var attrs = t.GetCustomAttributes(true).OfType<TAttribute>().ToList();

                if (attrs.Count == 0)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MetadataAttributeNotFound, typeof(TAttribute), t));
                }

                if (attrs.Count != 1)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, RegistrationExtensionsResources.MultipleMetadataAttributesSameType, typeof(TAttribute), t));
                }

                var attr = attrs[0];
                return metadataProperties.Select(p => new KeyValuePair<string, object?>(p.Name, p.GetValue(attr, null)));
            });
        }
    }
}
