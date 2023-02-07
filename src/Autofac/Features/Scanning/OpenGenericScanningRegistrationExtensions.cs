// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.OpenGenerics;
using Autofac.Util;

namespace Autofac.Features.Scanning;

/// <summary>
/// Helper methods to assist in scanning registration.
/// </summary>
internal static partial class ScanningRegistrationExtensions
{
    /// <summary>
    /// Register open generic types from the specified assemblies.
    /// </summary>
    /// <param name="builder">The container builder.</param>
    /// <param name="assemblies">The set of assemblies.</param>
    /// <returns>A registration builder.</returns>
    public static IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>
        RegisterOpenGenericAssemblyTypes(ContainerBuilder builder, params Assembly[] assemblies)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (assemblies == null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        var rb = new RegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle>(
            new TypedService(typeof(object)),
            new OpenGenericScanningActivatorData(),
            new DynamicRegistrationStyle());

        rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => ScanAssemblies(assemblies, cr, rb));

        return rb;
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
            IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
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

        registration.ActivatorData.ConfigurationActions.Add((t, rb) =>
        {
            var mapped = serviceMapping(t);
            var impl = rb.ActivatorData.ImplementationType;
            var applied = mapped.Where(s =>
            {
                if (s is IServiceWithType c)
                {
                    return impl.IsOpenGenericTypeOf(c.ServiceType);
                }

                return s != null;
            });
            rb.As(applied.ToArray());
        });

        return registration;
    }

    private static void ScanAssemblies(IEnumerable<Assembly> assemblies, IComponentRegistryBuilder cr, IRegistrationBuilder<object, OpenGenericScanningActivatorData, DynamicRegistrationStyle> rb)
    {
        rb.ActivatorData.Filters.Add(t =>
            rb.RegistrationData.Services.OfType<IServiceWithType>().All(swt =>
                t.IsOpenGenericTypeOf(swt.ServiceType)));

        var types = assemblies.SelectMany(a => a.GetPermittedTypesForAssemblyScanning())
            .Where(t => t.IsGenericTypeDefinition)
            .CanBeRegistered(rb.ActivatorData);

        static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> TypeBuilderFactory(Type type) => new RegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>(
                new TypedService(type),
                new ReflectionActivatorData(type),
                new DynamicRegistrationStyle());

        static void RegistrationSourceFactory(IComponentRegistryBuilder registry, IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle> data) => registry.AddRegistrationSource(new OpenGenericRegistrationSource(data.RegistrationData, data.ResolvePipeline, data.ActivatorData));

        ScanTypesTemplate(types, cr, rb, TypeBuilderFactory, RegistrationSourceFactory);
    }

    private static void ScanTypesTemplate<TActivatorData, TScanStyle, TRegistrationBuilderStyle>(
        IEnumerable<Type> types,
        IComponentRegistryBuilder cr,
        IRegistrationBuilder<object, BaseScanningActivatorData<TActivatorData, TScanStyle>, TRegistrationBuilderStyle> rb,
        Func<Type, IRegistrationBuilder<object, TActivatorData, TScanStyle>> scannedConstructorFunc,
        Action<IComponentRegistryBuilder, IRegistrationBuilder<object, TActivatorData, TScanStyle>> register)
        where TActivatorData : ReflectionActivatorData
    {
        foreach (var t in types)
        {
            var scanned = scannedConstructorFunc(t);

            scanned.ConfigureFrom(rb, t);

            if (scanned.RegistrationData.Services.Any())
            {
                register(cr, scanned);
            }
        }

        foreach (var postScanningCallback in rb.ActivatorData.PostScanningCallbacks)
        {
            postScanningCallback(cr);
        }
    }

    private static IEnumerable<Type> CanBeRegistered<TActivatorData, TStyle>(this IEnumerable<Type> types, BaseScanningActivatorData<TActivatorData, TStyle> activatorData)
        where TActivatorData : ReflectionActivatorData
    {
        // Issue #897: For back compat reasons we can't filter out
        // non-public types here. Folks use assembly scanning on their
        // own stuff, so encapsulation is a tricky thing to manage.
        // If people want only public types, a LINQ Where clause can be used.
        return types.Where(t => activatorData.Filters.All(p => p(t)));
    }

    private static void ConfigureFrom<TActivatorData, TScanStyle, TRegistrationBuilderStyle>(
        this IRegistrationBuilder<object, TActivatorData, TScanStyle> scanned,
        IRegistrationBuilder<object, BaseScanningActivatorData<TActivatorData, TScanStyle>, TRegistrationBuilderStyle> rb,
        Type type)
        where TActivatorData : ReflectionActivatorData
    {
        scanned
            .FindConstructorsWith(rb.ActivatorData.ConstructorFinder)
            .UsingConstructor(rb.ActivatorData.ConstructorSelector)
            .WithParameters(rb.ActivatorData.ConfiguredParameters)
            .WithProperties(rb.ActivatorData.ConfiguredProperties);

        // Copy middleware from the scanning registration.
        scanned.ResolvePipeline.UseRange(rb.ResolvePipeline.Middleware);

        scanned.RegistrationData.CopyFrom(rb.RegistrationData, false);

        foreach (var action in rb.ActivatorData.ConfigurationActions)
        {
            action(type, scanned);
        }
    }

    /// <summary>
    /// Filters the scanned types to include only those assignable to the provided.
    /// </summary>
    /// <typeparam name="TLimit">The limit type.</typeparam>
    /// <typeparam name="TRegistrationStyle">The registration style.</typeparam>
    /// <param name="registration">The registration builder.</param>
    /// <param name="openGenericServiceType">The type or interface which all classes must be assignable from.</param>
    /// <returns>The registration builder.</returns>
    public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
        AssignableTo<TLimit, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
            Type openGenericServiceType)
    {
        if (openGenericServiceType == null)
        {
            throw new ArgumentNullException(nameof(openGenericServiceType));
        }

        return registration
            .Where(candidateType => candidateType.IsOpenGenericTypeOf(openGenericServiceType))
            .As(candidateType => (Service)new TypedService(candidateType));
    }

    /// <summary>
    /// Filters the scanned types to include only those assignable to the provided.
    /// </summary>
    /// <typeparam name="TLimit">The limit type.</typeparam>
    /// <typeparam name="TRegistrationStyle">The registration style.</typeparam>
    /// <param name="registration">The registration builder.</param>
    /// <param name="openGenericServiceType">The type or interface which all classes must be assignable from.</param>
    /// <param name="serviceKey">The service key.</param>
    /// <returns>The registration builder.</returns>
    public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
        AssignableTo<TLimit, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
            Type openGenericServiceType,
            object serviceKey)
    {
        if (openGenericServiceType == null)
        {
            throw new ArgumentNullException(nameof(openGenericServiceType));
        }

        if (serviceKey == null)
        {
            throw new ArgumentNullException(nameof(serviceKey));
        }

        return AssignableTo(registration, openGenericServiceType, t => serviceKey);
    }

    /// <summary>
    /// Filters the scanned types to include only those assignable to the provided.
    /// </summary>
    /// <typeparam name="TLimit">The limit type.</typeparam>
    /// <typeparam name="TRegistrationStyle">The registration style.</typeparam>
    /// <param name="registration">The registration builder.</param>
    /// <param name="openGenericServiceType">The type or interface which all classes must be assignable from.</param>
    /// <param name="serviceKeyMapping">A function to determine the service key for a given type.</param>
    /// <returns>The registration builder.</returns>
    public static IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle>
        AssignableTo<TLimit, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, OpenGenericScanningActivatorData, TRegistrationStyle> registration,
            Type openGenericServiceType,
            Func<Type, object> serviceKeyMapping)
    {
        if (openGenericServiceType == null)
        {
            throw new ArgumentNullException(nameof(openGenericServiceType));
        }

        return registration
            .Where(candidateType => candidateType.IsOpenGenericTypeOf(openGenericServiceType))
            .As(candidateType => (Service)new KeyedService(serviceKeyMapping(candidateType), candidateType));
    }
}
