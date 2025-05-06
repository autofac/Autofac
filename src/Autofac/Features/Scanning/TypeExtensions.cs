// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Features.Scanning;

/// <summary>
/// Extension methods for working with types in a scanning context.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// Filters a list of types using the set of filters associated with the provided activator data.
    /// </summary>
    /// <typeparam name="TActivatorData">Activator builder type.</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
    /// <param name="types">The set of types to filter.</param>
    /// <param name="activatorData">The activator data with filters to run on the type list.</param>
    /// <returns>
    /// A filtered list of types that can be registered according to the activator data.
    /// </returns>
    internal static IEnumerable<Type> AllowedByActivatorFilters<TActivatorData, TRegistrationStyle>(this IEnumerable<Type> types, BaseScanningActivatorData<TActivatorData, TRegistrationStyle> activatorData)
        where TActivatorData : ReflectionActivatorData
    {
        return types.Where(t => activatorData.Filters.All(p => p(t)));
    }

    /// <summary>
    /// Filters a list of types down to only those concrete types allowed by scanning registrations.
    /// </summary>
    /// <param name="types">
    /// The types to check.
    /// </param>
    /// <returns>
    /// A filtered set of types that remove non-concrete types (interfaces, abstract classes, etc.).
    /// </returns>
    internal static IEnumerable<Type> WhichAreAllowedThroughScanning(this IEnumerable<Type> types) => types.Where(t => t.MayAllowReflectionActivation());

    /// <summary>
    /// Filters a list of types down to only those that are concrete
    /// (<see cref="InternalTypeExtensions.MayAllowReflectionActivation"/>), not open generic,
    /// and allowed by the provided activator data. Registers those types into
    /// the provided
    /// registry.
    /// </summary>
    /// <param name="types">
    /// The set of types to filter and register.
    /// </param>
    /// <param name="cr">
    /// The registry builder into which the filtered types should be registered.
    /// </param>
    /// <param name="rb">
    /// A "template" registration builder that is used to provide activator data
    /// filters and serve as the basis for individual component registrations.
    /// </param>
    internal static void FilterAndRegisterConcreteTypes(this IEnumerable<Type> types, IComponentRegistryBuilder cr, IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb)
    {
        var closedTypes = types.WhichAreAllowedThroughScanning()
            .Where(t => !t.IsGenericTypeDefinition)
            .AllowedByActivatorFilters(rb.ActivatorData);

        rb.ActivatorData.Filters.Add(t =>
            rb.RegistrationData.Services.OfType<IServiceWithType>().All(swt =>
                swt.ServiceType.IsAssignableFrom(t)));

        static IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> TypeBuilderFactory(Type type) => RegistrationBuilder.ForType(type);

        static void SingleComponentRegistration(IComponentRegistryBuilder registry, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> data) => RegistrationBuilder.RegisterSingleComponent(registry, data);

        closedTypes.RegisterUsingTemplate(cr, rb, TypeBuilderFactory, SingleComponentRegistration);
    }

    /// <summary>
    /// Executes a "template" to convert a set of types into a set of
    /// registrations and then register them with the container. This is used
    /// for both concrete and open generic type scanning, so the template
    /// differs based on the thing being scanned.
    /// </summary>
    /// <typeparam name="TActivatorData">
    /// Activator data type. Usually <see cref="ConcreteReflectionActivatorData"/>
    /// for concrete types or <see cref="ReflectionActivatorData"/> for open generics.
    /// </typeparam>
    /// <typeparam name="TScanStyle">
    /// Scanning style type. Usually <see cref="SingleRegistrationStyle"/> for
    /// concrete types or <see cref="DynamicRegistrationStyle"/> for open
    /// generics.
    /// </typeparam>
    /// <typeparam name="TRegistrationBuilderStyle">
    /// Registration style type. Usually <see cref="DynamicRegistrationStyle"/>
    /// regardless of what's being registered.
    /// </typeparam>
    /// <param name="types">
    /// The set of types to scan and register.
    /// </param>
    /// <param name="cr">
    /// The registry builder into which the built component registrations should
    /// be added.
    /// </param>
    /// <param name="rb">
    /// A base/template registration builder for scanned registrations.
    /// </param>
    /// <param name="scannedConstructorFunc">
    /// A function that takes a type and generates the registration for that
    /// type. Will be configured based on <paramref name="rb"/>.
    /// </param>
    /// <param name="register">
    /// A function that takes the built registration and adds it to the
    /// <paramref name="cr"/> registry builder.
    /// </param>
    internal static void RegisterUsingTemplate<TActivatorData, TScanStyle, TRegistrationBuilderStyle>(
        this IEnumerable<Type> types,
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

    /// <summary>
    /// Copies all the activator data from a "template" registration into a
    /// scanned registration. This is how constructor finders, parameters, and
    /// other registration data gets from the top level registration call into
    /// individual type registrations.
    /// </summary>
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
}
