// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Globalization;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OpenGenerics;

/// <summary>
/// Helper functions for binding open generic implementations to a known implementation type.
/// </summary>
internal static class OpenGenericServiceBinder
{
    /// <summary>
    /// Given a closed generic service (that is being requested), creates a closed generic implementation type
    /// and associated services from the open generic implementation and services.
    /// </summary>
    /// <param name="serviceWithType">The closed generic service to bind.</param>
    /// <param name="configuredOpenGenericServices">The set of configured open generic services.</param>
    /// <param name="openGenericImplementationType">The implementation type of the open generic.</param>
    /// <param name="constructedImplementationType">The built closed generic implementation type.</param>
    /// <param name="constructedServices">The built closed generic services.</param>
    /// <returns>True if the closed generic service can be bound. False otherwise.</returns>
    [SuppressMessage("CA1851", "CA1851", Justification = "The CPU cost in enumerating the list of services is low, while allocating a new list saves little in CPU but costs a lot in allocations.")]
    public static bool TryBindOpenGenericTypedService(
        IServiceWithType serviceWithType,
        IEnumerable<Service> configuredOpenGenericServices,
        Type openGenericImplementationType,
        [NotNullWhen(returnValue: true)] out Type? constructedImplementationType,
        [NotNullWhen(returnValue: true)] out Service[]? constructedServices)
    {
        if (serviceWithType.ServiceType.IsGenericType && !serviceWithType.ServiceType.IsGenericTypeDefinition)
        {
            var definitionService = (IServiceWithType)serviceWithType.ChangeType(serviceWithType.ServiceType.GetGenericTypeDefinition());
            var serviceGenericArguments = serviceWithType.ServiceType.GetGenericArguments();

            if (configuredOpenGenericServices.OfType<IServiceWithType>().Any(s => s.Equals(definitionService)))
            {
                var implementorGenericArguments = TryMapImplementationGenericArguments(
                    openGenericImplementationType, serviceWithType.ServiceType, definitionService.ServiceType, serviceGenericArguments);

                if (implementorGenericArguments.All(a => a != null) &&
                    openGenericImplementationType.IsCompatibleWithGenericParameterConstraints(implementorGenericArguments!))
                {
                    var constructedImplementationTypeTmp = openGenericImplementationType.MakeGenericType(implementorGenericArguments!);

                    var implementedServices = configuredOpenGenericServices
                        .OfType<IServiceWithType>()
                        .Where(s => s.ServiceType.GetGenericArguments().Length == serviceGenericArguments.Length)
                        .Select(s => new { ServiceWithType = s, GenericService = s.ServiceType.MakeGenericType(serviceGenericArguments) })
                        .Where(p => p.GenericService.IsAssignableFrom(constructedImplementationTypeTmp))
                        .Select(p => p.ServiceWithType.ChangeType(p.GenericService))
                        .ToArray();

                    if (implementedServices.Length > 0)
                    {
                        constructedImplementationType = constructedImplementationTypeTmp;
                        constructedServices = implementedServices;
                        return true;
                    }
                }
            }
        }

        constructedImplementationType = null;
        constructedServices = null;
        return false;
    }

    private static Type GetGenericTypeDefinition(Type type) => ReflectionCacheSet.Shared.Internal.GenericTypeDefinitionByType.GetOrAdd(type, static t => t.GetGenericTypeDefinition());

    /// <summary>
    /// Given a closed generic service (that is being requested), creates a regular delegate callback
    /// and associated services from the open generic delegate and services.
    /// </summary>
    /// <param name="serviceWithType">The closed generic service to bind.</param>
    /// <param name="configuredOpenGenericServices">The set of configured open generic services.</param>
    /// <param name="openGenericFactory">Delegate responsible for generating an instance of a closed generic based on the open generic type being registered.</param>
    /// <param name="constructedFactory">The built closed generic implementation type.</param>
    /// <param name="constructedServices">The built closed generic services.</param>
    /// <returns>True if the closed generic service can be bound. False otherwise.</returns>
    [SuppressMessage("CA1851", "CA1851", Justification = "The CPU cost in enumerating the list of services is low, while allocating a new list saves little in CPU but costs a lot in allocations.")]
    public static bool TryBindOpenGenericDelegateService(
        IServiceWithType serviceWithType,
        IEnumerable<Service> configuredOpenGenericServices,
        Func<IComponentContext, Type[], IEnumerable<Parameter>, object> openGenericFactory,
        [NotNullWhen(returnValue: true)] out Func<IComponentContext, IEnumerable<Parameter>, object>? constructedFactory,
        [NotNullWhen(returnValue: true)] out Service[]? constructedServices)
    {
        var serviceWithTypeServiceType = serviceWithType.ServiceType;
        if (serviceWithTypeServiceType.IsGenericType && !serviceWithTypeServiceType.IsGenericTypeDefinition)
        {
            var definitionService = (IServiceWithType)serviceWithType.ChangeType(GetGenericTypeDefinition(serviceWithTypeServiceType));
            var serviceGenericArguments = serviceWithTypeServiceType.GetGenericArguments();

            foreach (var s in configuredOpenGenericServices.OfType<IServiceWithType>())
            {
                if (s.Equals(definitionService))
                {
                    constructedFactory = (ctx, parameters) => openGenericFactory(ctx, serviceGenericArguments, parameters);

                    var serviceGenericArgumentsLength = serviceGenericArguments.Length;
                    var implementedServices = new List<Service>();
                    foreach (var service in configuredOpenGenericServices.OfType<IServiceWithType>())
                    {
                        var serviceType = service.ServiceType;
                        if (serviceType.GetGenericArguments().Length == serviceGenericArgumentsLength)
                        {
                            var genericService = serviceType.MakeGenericType(serviceGenericArguments);
                            implementedServices.Add(service.ChangeType(genericService));
                        }
                    }

                    constructedServices = implementedServices.ToArray();
                    return true;
                }
            }
        }

        constructedFactory = null;
        constructedServices = null;
        return false;
    }

    private static Type?[] TryMapImplementationGenericArguments(Type implementationType, Type serviceType, Type serviceTypeDefinition, Type[] serviceGenericArguments)
    {
        if (serviceTypeDefinition == implementationType)
        {
            return serviceGenericArguments;
        }

        if (!serviceType.IsInterface)
        {
            /* Issue #1315: We walk backwards in the inheritance hierarchy to
             * find the open generic because that's the only way to ensure the
             * generic argument names match.
             *
             * If you have a class BaseClass<T1, T2> and a derived class
             * DerivedClass<A1, A2> : BaseClass<A2, A1> then then arguments need
             * to line up name-wise like...
             * A2 -> T1
             * A1 -> T2
             *
             * Having those symbols aligned means that, later, we can do a
             * name-based match to populate the generic arguments.
             *
             * If you do a typeof(BaseClass<,>) then the generic arguments are
             * named T1 and T2, which doesn't help us line up the arguments in
             * the derived class because the names and order have changed.
             * However, if you start at the derived class and walk backwards up
             * the inheritance hierarchy then instead of getting the original
             * T1/T2 naming, we get the names A1/A2 - the symbols as lined up by
             * the compiler.
             *
             * This is the same reason why Type.GetInterfaces() "just works" -
             * it's the generic in relation to the derived/implementing type
             * rather than just typeof(MyInterface<,>), so the names all line
             * up.
             */
            var baseType = GetGenericBaseType(implementationType, serviceTypeDefinition);
            if (baseType == null)
            {
                // If it's not an interface, the implementation type MUST have derived from
                // the generic service type at some point or there's no way to cast.
                return Array.Empty<Type>();
            }

            return TryFindServiceArgumentsForImplementation(
                    implementationType,
                    serviceGenericArguments,
                    baseType.GenericTypeArguments);
        }

        var availableArguments = GetInterfaces(implementationType, serviceType)
            .Select(t => TryFindServiceArgumentsForImplementation(
                implementationType,
                serviceGenericArguments,
                t.GenericTypeArguments))
            .ToArray();

        var exactMatch = availableArguments.FirstOrDefault(a => a.SequenceEqual(serviceGenericArguments));
        return exactMatch ?? availableArguments[0];
    }

    private static Type?[] TryFindServiceArgumentsForImplementation(Type implementationType, IEnumerable<Type> serviceGenericArguments, IEnumerable<Type> serviceArgumentDefinitions)
    {
        var serviceArgumentDefinitionToArgumentMapping =
            serviceArgumentDefinitions.Zip(serviceGenericArguments, (a, b) => new KeyValuePair<Type, Type>(a, b));

        var implementationGenericArgumentDefinitions = implementationType.GetGenericArguments();
        return implementationGenericArgumentDefinitions
            .Select(implementationGenericArgumentDefinition => TryFindServiceArgumentForImplementationArgumentDefinition(
                implementationGenericArgumentDefinition, serviceArgumentDefinitionToArgumentMapping))
            .ToArray();
    }

    private static Type? GetGenericBaseType(Type implementationType, Type serviceTypeDefinition)
    {
        var baseType = implementationType.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == serviceTypeDefinition)
            {
                break;
            }

            baseType = baseType.BaseType;
        }

        return baseType;
    }

    private static Type[] GetInterfaces(Type implementationType, Type serviceType) =>
        implementationType.GetInterfaces()
            .Where(i => i.Name == serviceType.Name && i.Namespace == serviceType.Namespace)
            .ToArray();

    [SuppressMessage("CA1851", "CA1851", Justification = "The CPU cost in enumerating the list of services is low, while allocating a new list saves little in CPU but costs a lot in allocations.")]
    private static Type? TryFindServiceArgumentForImplementationArgumentDefinition(Type implementationGenericArgumentDefinition, IEnumerable<KeyValuePair<Type, Type>> serviceArgumentDefinitionToArgument)
    {
        var matchingRegularType = serviceArgumentDefinitionToArgument
            .Where(argdef => !argdef.Key.IsGenericType && implementationGenericArgumentDefinition.Name == argdef.Key.Name)
            .Select(argdef => argdef.Value)
            .FirstOrDefault();

        if (matchingRegularType != null)
        {
            return matchingRegularType;
        }

        return serviceArgumentDefinitionToArgument
            .Where(argdef => argdef.Key.IsGenericType && argdef.Value.GenericTypeArguments.Length > 0)
            .Select(argdef => TryFindServiceArgumentForImplementationArgumentDefinition(
                implementationGenericArgumentDefinition, argdef.Key.GenericTypeArguments.Zip(
                    argdef.Value.GenericTypeArguments, (a, b) => new KeyValuePair<Type, Type>(a, b))))
            .FirstOrDefault(x => x != null);
    }

    /// <summary>
    /// Throws an exception if an open generic implementation type cannot implement the set of specified open services.
    /// </summary>
    /// <param name="implementationType">The open generic implementation type.</param>
    /// <param name="services">The set of open generic services.</param>
    public static void EnforceBindable(Type implementationType, IEnumerable<Service> services)
    {
        if (implementationType == null)
        {
            throw new ArgumentNullException(nameof(implementationType));
        }

        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (!implementationType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ImplementorMustBeOpenGenericTypeDefinition, implementationType));
        }

        foreach (var service in services.OfType<IServiceWithType>())
        {
            if (!service.ServiceType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ServiceTypeMustBeOpenGenericTypeDefinition, service));
            }

            if (service.ServiceType.IsInterface)
            {
                if (GetInterfaces(implementationType, service.ServiceType).Length == 0)
                {
                    var message = string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.ImplementorDoesntImplementService, implementationType.FullName, service.ServiceType.FullName);
                    throw new InvalidOperationException(message);
                }
            }
            else
            {
                if (!Traverse.Across(implementationType, t => t.BaseType!).Any(t => IsCompatibleGenericClassDefinition(t, service.ServiceType)))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericServiceBinderResources.TypesAreNotConvertible, implementationType, service));
                }
            }
        }
    }

    private static bool IsCompatibleGenericClassDefinition(Type implementor, Type serviceType)
    {
        return implementor == serviceType || (implementor.IsGenericType && implementor.GetGenericTypeDefinition() == serviceType);
    }
}
