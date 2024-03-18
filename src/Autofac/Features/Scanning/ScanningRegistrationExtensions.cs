// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Features.Scanning;

/// <summary>
/// Helper methods to assist in scanning registration.
/// </summary>
internal static class ScanningRegistrationExtensions
{
    /// <summary>
    /// Register types from the specified assemblies.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <param name="assemblies">The set of assemblies.</param>
    /// <returns>A registration builder.</returns>
    public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
        ScanAndRegisterAssemblyTypes(this ContainerBuilder builder, params Assembly[] assemblies)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (assemblies == null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        var rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(
            new TypedService(typeof(object)),
            new ScanningActivatorData(),
            new DynamicRegistrationStyle());

        rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => assemblies.ScanAssemblies(cr, rb));

        return rb;
    }

    /// <summary>
    /// Register the specified types.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <param name="types">The set of types.</param>
    /// <returns>A registration builder.</returns>
    public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
        ScanAndRegisterTypes(this ContainerBuilder builder, params Type[] types)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (types == null)
        {
            throw new ArgumentNullException(nameof(types));
        }

        var rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(
            new TypedService(typeof(object)),
            new ScanningActivatorData(),
            new DynamicRegistrationStyle());

        rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => TypeExtensions.FilterAndRegisterConcreteTypes(types, cr, rb));

        return rb;
    }

    /// <summary>
    /// Configures the scanning registration builder to register all closed types of the specified open generic.
    /// </summary>
    /// <typeparam name="TLimit">The limit type.</typeparam>
    /// <typeparam name="TScanningActivatorData">The activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">The registration style.</typeparam>
    /// <param name="registration">The registration builder.</param>
    /// <param name="openGenericServiceType">The open generic to register closed types of.</param>
    /// <returns>The registration builder.</returns>
    public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
        AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
            Type openGenericServiceType)
        where TScanningActivatorData : ScanningActivatorData
    {
        if (openGenericServiceType == null)
        {
            throw new ArgumentNullException(nameof(openGenericServiceType));
        }

        return registration
            .Where(candidateType => candidateType.IsClosedTypeOf(openGenericServiceType))
            .As(candidateType => candidateType.GetTypesThatClose(openGenericServiceType)
                    .Select(t => (Service)new TypedService(t)));
    }

    /// <summary>
    /// Configures the scanning registration builder to register all closed types of the specified open generic as a keyed service.
    /// </summary>
    /// <typeparam name="TLimit">The limit type.</typeparam>
    /// <typeparam name="TScanningActivatorData">The activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">The registration style.</typeparam>
    /// <param name="registration">The registration builder.</param>
    /// <param name="openGenericServiceType">The open generic to register closed types of.</param>
    /// <param name="serviceKey">The service key.</param>
    /// <returns>The registration builder.</returns>
    public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
        AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
            Type openGenericServiceType,
            object serviceKey)
        where TScanningActivatorData : ScanningActivatorData
    {
        if (openGenericServiceType == null)
        {
            throw new ArgumentNullException(nameof(openGenericServiceType));
        }

        if (serviceKey == null)
        {
            throw new ArgumentNullException(nameof(serviceKey));
        }

        return AsClosedTypesOf(registration, openGenericServiceType, t => serviceKey);
    }

    /// <summary>
    /// Configures the scanning registration builder to register all closed types of the specified open generic as a keyed service.
    /// </summary>
    /// <typeparam name="TLimit">The limit type.</typeparam>
    /// <typeparam name="TScanningActivatorData">The activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">The registration style.</typeparam>
    /// <param name="registration">The registration builder.</param>
    /// <param name="openGenericServiceType">The open generic to register closed types of.</param>
    /// <param name="serviceKeyMapping">A function to determine the service key for a given type.</param>
    /// <returns>The registration builder.</returns>
    public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
        AsClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
            Type openGenericServiceType,
            Func<Type, object> serviceKeyMapping)
        where TScanningActivatorData : ScanningActivatorData
    {
        if (openGenericServiceType == null)
        {
            throw new ArgumentNullException(nameof(openGenericServiceType));
        }

        return registration
            .Where(candidateType => candidateType.IsClosedTypeOf(openGenericServiceType))
            .As(candidateType => candidateType.GetTypesThatClose(openGenericServiceType)
                    .Select(t => (Service)new KeyedService(serviceKeyMapping(candidateType), t)));
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
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        registration.ActivatorData.Filters.Add(t => type.IsAssignableFrom(t));
        return registration;
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
            IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
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

        registration.ActivatorData.ConfigurationActions.Add((t, rb) =>
        {
            var mapped = serviceMapping(t);
            var impl = rb.ActivatorData.ImplementationType;
            var applied = mapped.Where(s =>
                {
                    if (s is IServiceWithType c)
                    {
                        return c.ServiceType.IsAssignableFrom(impl);
                    }

                    return s is not null;
                });
            rb.As(applied.ToArray());
        });

        return registration;
    }

    /// <summary>
    /// Specifies that the components being registered should only be made the default for services
    /// that have not already been registered.
    /// </summary>
    /// <typeparam name="TLimit">Registration limit type.</typeparam>
    /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
    /// <param name="registration">Registration to set service mapping on.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
        PreserveExistingDefaults<TLimit, TScanningActivatorData, TRegistrationStyle>(
        IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration)
        where TScanningActivatorData : ScanningActivatorData
    {
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        registration.ActivatorData.ConfigurationActions.Add((t, r) => r.PreserveExistingDefaults());

        return registration;
    }
}
