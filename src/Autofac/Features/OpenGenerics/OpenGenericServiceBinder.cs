// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OpenGenerics;

/// <summary>
/// Helper functions for binding open generic implementations to a known implementation type.
/// </summary>
internal static class OpenGenericServiceBinder
{
    private const string OpenGenericDynamicCodeWarning = "Binding open generic types constructs closed generic types at runtime via MakeGenericType, which may require dynamic code generation when closed over value types and is not compatible with native AOT.";

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
    [RequiresDynamicCode(OpenGenericDynamicCodeWarning)]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2055:MakeGenericType",
        Justification = "Constructs the closed implementation and service types from the open generic registration's runtime type arguments. The implementation type's activatable members are preserved by its [DynamicallyAccessedMembers] annotation; the consumer that registered the open generic is responsible for the closed type.")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2072:UnrecognizedReflectionPattern",
        Justification = "When the requested service's definition is the implementation type, the requested service type is the closed implementation type that MakeGenericType would otherwise construct, so the same responsibility for the closed type applies.")]
    public static bool TryBindOpenGenericTypedService(
        IServiceWithType serviceWithType,
        IEnumerable<Service> configuredOpenGenericServices,
        [DynamicallyAccessedMembers(ActivatorMemberTypes.ActivatedType)] Type openGenericImplementationType,
        [DynamicallyAccessedMembers(ActivatorMemberTypes.ActivatedType)][NotNullWhen(returnValue: true)] out Type? constructedImplementationType,
        [NotNullWhen(returnValue: true)] out Service[]? constructedServices)
    {
        var serviceType = serviceWithType.ServiceType;
        if (IsClosedGenericType(serviceType))
        {
            var serviceTypeDefinition = serviceType.GetGenericTypeDefinition();

            if (IsConfigured(configuredOpenGenericServices, serviceWithType, serviceTypeDefinition))
            {
                var serviceGenericArguments = serviceType.GetGenericArguments();
                var implementation = OpenGenericImplementationInfo.For(openGenericImplementationType);
                var implementorGenericArguments = TryMapImplementationGenericArguments(
                    openGenericImplementationType, implementation, serviceType, serviceTypeDefinition, serviceGenericArguments);

                if (AllMapped(implementorGenericArguments) &&
                    InternalTypeExtensions.IsCompatibleWithGenericParameterConstraints(
                        implementation.GenericArguments, implementation.GenericParameterConstraints, implementorGenericArguments!))
                {
                    // Closing the implementation type over the requested service's own arguments gives back the requested type.
                    var constructedImplementationTypeTmp = serviceTypeDefinition == openGenericImplementationType
                        ? serviceType
                        : openGenericImplementationType.MakeGenericType(implementorGenericArguments!);

                    var implementedServices = BuildImplementedServices(
                        configuredOpenGenericServices, serviceWithType, serviceTypeDefinition, serviceGenericArguments, constructedImplementationTypeTmp);

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
    [RequiresDynamicCode(OpenGenericDynamicCodeWarning)]
    public static bool TryBindOpenGenericDelegateService(
        IServiceWithType serviceWithType,
        IEnumerable<Service> configuredOpenGenericServices,
        Func<IComponentContext, Type[], IEnumerable<Parameter>, object> openGenericFactory,
        [NotNullWhen(returnValue: true)] out Func<IComponentContext, IEnumerable<Parameter>, object>? constructedFactory,
        [NotNullWhen(returnValue: true)] out Service[]? constructedServices)
    {
        var serviceWithTypeServiceType = serviceWithType.ServiceType;
        if (!IsClosedGenericType(serviceWithTypeServiceType))
        {
            constructedFactory = null;
            constructedServices = null;
            return false;
        }

        var serviceTypeDefinition = GetGenericTypeDefinition(serviceWithTypeServiceType);

        if (!IsConfigured(configuredOpenGenericServices, serviceWithType, serviceTypeDefinition))
        {
            constructedFactory = null;
            constructedServices = null;
            return false;
        }

        var serviceGenericArguments = serviceWithTypeServiceType.GetGenericArguments();
        constructedFactory = CloseFactory(openGenericFactory, serviceGenericArguments);
        constructedServices = BuildImplementedServices(configuredOpenGenericServices, serviceWithType, serviceTypeDefinition, serviceGenericArguments, null);
        return true;
    }

    /// <summary>
    /// Throws an exception if an open generic implementation type cannot implement the set of specified open services.
    /// </summary>
    /// <param name="implementationType">The open generic implementation type.</param>
    /// <param name="services">The set of open generic services.</param>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2067:UnrecognizedReflectionPattern",
        Justification = "Inspects the interfaces of the open generic implementation type to validate it can implement the configured services. The implementation type comes from an open generic registration (RegisterGeneric / open generic scanning), whose consumer is responsible for preserving its interfaces.")]
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

    // Builds the closure here rather than in the caller, which would allocate it on every call, even for services it doesn't bind.
    private static Func<IComponentContext, IEnumerable<Parameter>, object> CloseFactory(
        Func<IComponentContext, Type[], IEnumerable<Parameter>, object> openGenericFactory,
        Type[] serviceGenericArguments)
        => (ctx, parameters) => openGenericFactory(ctx, serviceGenericArguments, parameters);

    private static bool IsClosedGenericType(Type serviceType)
        => serviceType.IsGenericType && !serviceType.IsGenericTypeDefinition;

    private static bool IsConfigured(IEnumerable<Service> configuredOpenGenericServices, IServiceWithType serviceWithType, Type serviceTypeDefinition)
    {
        // Built only to compare against configured services that aren't typed or keyed services.
        IServiceWithType? definitionService = null;

        // The registration sources pass an array, which can be looped over without allocating an enumerator.
        if (configuredOpenGenericServices is Service[] services)
        {
            foreach (var service in services)
            {
                if (IsDefinitionService(service, serviceWithType, serviceTypeDefinition, ref definitionService))
                {
                    return true;
                }
            }

            return false;
        }

        foreach (var service in configuredOpenGenericServices)
        {
            if (IsDefinitionService(service, serviceWithType, serviceTypeDefinition, ref definitionService))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether a configured service equals the requested service changed to the definition type. When both are
    /// typed or keyed services, that is the comparison their Equals would make, so the definition service isn't built.
    /// </summary>
    private static bool IsDefinitionService(
        Service service,
        IServiceWithType serviceWithType,
        Type serviceTypeDefinition,
        ref IServiceWithType? definitionService) => (service, serviceWithType) switch
        {
            (TypedService typed, TypedService) => typed.ServiceType == serviceTypeDefinition,
            (KeyedService keyed, KeyedService keyedRequest) => keyed.ServiceKey.Equals(keyedRequest.ServiceKey) && keyed.ServiceType == serviceTypeDefinition,
            (TypedService or KeyedService, TypedService or KeyedService) => false,
            (IServiceWithType configured, _) => configured.Equals(definitionService ??= (IServiceWithType)serviceWithType.ChangeType(serviceTypeDefinition)),
            _ => false,
        };

    private static bool AllMapped(Type?[] genericArguments)
    {
        foreach (var genericArgument in genericArguments)
        {
            if (genericArgument is null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Closes each configured open generic service over the requested service's generic arguments. When an
    /// implementation type is given, services it can't be assigned to are left out.
    /// </summary>
    [RequiresDynamicCode(OpenGenericDynamicCodeWarning)]
    private static Service[] BuildImplementedServices(
        IEnumerable<Service> configuredOpenGenericServices,
        IServiceWithType serviceWithType,
        Type serviceTypeDefinition,
        Type[] serviceGenericArguments,
        Type? constructedImplementationType)
    {
        Service? firstService = null;
        List<Service>? implementedServices = null;

        if (configuredOpenGenericServices is Service[] services)
        {
            foreach (var service in services)
            {
                AddImplementedService(service);
            }
        }
        else
        {
            foreach (var service in configuredOpenGenericServices)
            {
                AddImplementedService(service);
            }
        }

        if (implementedServices is not null)
        {
            return implementedServices.ToArray();
        }

        return firstService is null ? Array.Empty<Service>() : new[] { firstService };

        void AddImplementedService(Service service)
        {
            var implementedService = TryCloseService(service, serviceWithType, serviceTypeDefinition, serviceGenericArguments, constructedImplementationType);
            if (implementedService is null)
            {
                return;
            }

            if (firstService is null)
            {
                firstService = implementedService;
            }
            else
            {
                implementedServices ??= new List<Service> { firstService };
                implementedServices.Add(implementedService);
            }
        }
    }

    [RequiresDynamicCode(OpenGenericDynamicCodeWarning)]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2055:MakeGenericType",
        Justification = "Constructs the closed service types from the open generic registration's runtime type arguments. The consumer that registered the open generic is responsible for preserving the closed types.")]
    private static Service? TryCloseService(
        Service service,
        IServiceWithType requestedService,
        Type serviceTypeDefinition,
        Type[] serviceGenericArguments,
        Type? constructedImplementationType)
    {
        if (service is not IServiceWithType serviceWithType)
        {
            return null;
        }

        var openServiceType = serviceWithType.ServiceType;
        var serviceType = requestedService.ServiceType;
        Type genericService;
        if (openServiceType == serviceTypeDefinition)
        {
            // Closing the requested service's definition over its own arguments gives back the requested type.
            genericService = serviceType;
        }
        else if (openServiceType.GetGenericArguments().Length == serviceGenericArguments.Length)
        {
            genericService = openServiceType.MakeGenericType(serviceGenericArguments);
        }
        else
        {
            return null;
        }

        if (constructedImplementationType is not null && !genericService.IsAssignableFrom(constructedImplementationType))
        {
            return null;
        }

        if (genericService == serviceType && AsRequestedService(service, requestedService) is { } sameService)
        {
            return sameService;
        }

        return serviceWithType.ChangeType(genericService);
    }

    /// <summary>
    /// Gets the requested service when closing the configured service over the requested type would build an
    /// equal one: both typed, or both keyed with the same key object.
    /// </summary>
    private static Service? AsRequestedService(Service service, IServiceWithType requestedService) => (service, requestedService) switch
    {
        (TypedService, TypedService typedRequest) => typedRequest,
        (KeyedService keyed, KeyedService keyedRequest) when ReferenceEquals(keyed.ServiceKey, keyedRequest.ServiceKey) => keyedRequest,
        _ => null,
    };

    private static Type GetGenericTypeDefinition(Type type) => ReflectionCacheSet.Shared.Internal.GenericTypeDefinitionByType.GetOrAdd(type, static t => t.GetGenericTypeDefinition());

    private static Type?[] TryMapImplementationGenericArguments(
        Type implementationType,
        OpenGenericImplementationInfo implementation,
        Type serviceType,
        Type serviceTypeDefinition,
        Type[] serviceGenericArguments)
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
            var baseTypeGenericArguments = GetGenericBaseTypeArguments(implementation, serviceTypeDefinition);
            if (baseTypeGenericArguments == null)
            {
                // If it's not an interface, the implementation type MUST have derived from
                // the generic service type at some point or there's no way to cast.
                return Array.Empty<Type>();
            }

            return TryFindServiceArgumentsForImplementation(
                    implementation.GenericArguments,
                    serviceGenericArguments,
                    baseTypeGenericArguments);
        }

        Type?[]? firstMatch = null;
        Type?[]? completeMatch = null;

        for (var i = 0; i < implementation.Interfaces.Length; i++)
        {
            var implementedInterface = implementation.Interfaces[i];
            if (implementedInterface.Name != serviceType.Name || implementedInterface.Namespace != serviceType.Namespace)
            {
                continue;
            }

            var availableArguments = TryFindServiceArgumentsForImplementation(
                implementation.GenericArguments,
                serviceGenericArguments,
                implementation.InterfaceGenericArguments[i]);

            if (availableArguments.SequenceEqual(serviceGenericArguments))
            {
                return availableArguments;
            }

            firstMatch ??= availableArguments;

            // When no exact match exists, prefer the first candidate that produces a complete
            // mapping (no null entries), so that a non-mappable interface appearing earlier in
            // the interface list does not shadow a valid one that appears later.
            if (completeMatch is null && AllMapped(availableArguments))
            {
                completeMatch = availableArguments;
            }
        }

        // Fall back to the first candidate when no candidate fully maps (preserves prior behavior
        // for genuinely un-bindable registrations, where the caller's null check handles it).
        // EnforceBindable has checked that at least one interface matches.
        return completeMatch ?? firstMatch!;
    }

    private static Type?[] TryFindServiceArgumentsForImplementation(Type[] implementationGenericArgumentDefinitions, Type[] serviceGenericArguments, Type[] serviceArgumentDefinitions)
    {
        if (MapsByPosition(implementationGenericArgumentDefinitions, serviceGenericArguments, serviceArgumentDefinitions))
        {
            return serviceGenericArguments;
        }

        var implementorGenericArguments = new Type?[implementationGenericArgumentDefinitions.Length];
        for (var i = 0; i < implementorGenericArguments.Length; i++)
        {
            implementorGenericArguments[i] = TryFindServiceArgumentForImplementationArgumentDefinition(
                implementationGenericArgumentDefinitions[i], serviceArgumentDefinitions, serviceGenericArguments);
        }

        return implementorGenericArguments;
    }

    /// <summary>
    /// Checks whether the service passes the implementation's generic parameters through in order, so that each one
    /// maps to the service argument at its position. A parameter only maps by position when no earlier parameter has
    /// the same name, since the mapping matches parameters by name.
    /// </summary>
    private static bool MapsByPosition(Type[] implementationGenericArgumentDefinitions, Type[] serviceGenericArguments, Type[] serviceArgumentDefinitions)
    {
        if (serviceArgumentDefinitions.Length != implementationGenericArgumentDefinitions.Length ||
            serviceGenericArguments.Length != implementationGenericArgumentDefinitions.Length)
        {
            return false;
        }

        for (var i = 0; i < serviceArgumentDefinitions.Length; i++)
        {
            if (serviceArgumentDefinitions[i] != implementationGenericArgumentDefinitions[i])
            {
                return false;
            }

            for (var j = 0; j < i; j++)
            {
                if (serviceArgumentDefinitions[j].Name == serviceArgumentDefinitions[i].Name)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static Type[]? GetGenericBaseTypeArguments(OpenGenericImplementationInfo implementation, Type serviceTypeDefinition)
    {
        foreach (var baseType in implementation.GenericBaseTypes)
        {
            if (baseType.Definition == serviceTypeDefinition)
            {
                return baseType.GenericArguments;
            }
        }

        return null;
    }

    private static Type[] GetInterfaces([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type implementationType, Type serviceType)
        => implementationType.GetInterfaces()
            .Where(i => i.Name == serviceType.Name && i.Namespace == serviceType.Namespace)
            .ToArray();

    /// <summary>
    /// Finds the argument that an implementation generic argument definition maps to, pairing each service
    /// argument definition with the argument at the same position and searching nested generics when no
    /// definition matches by name.
    /// </summary>
    private static Type? TryFindServiceArgumentForImplementationArgumentDefinition(Type implementationGenericArgumentDefinition, Type[] serviceArgumentDefinitions, Type[] serviceArguments)
    {
        var pairCount = Math.Min(serviceArgumentDefinitions.Length, serviceArguments.Length);

        for (var i = 0; i < pairCount; i++)
        {
            var serviceArgumentDefinition = serviceArgumentDefinitions[i];
            if (!serviceArgumentDefinition.IsGenericType && implementationGenericArgumentDefinition.Name == serviceArgumentDefinition.Name)
            {
                return serviceArguments[i];
            }
        }

        for (var i = 0; i < pairCount; i++)
        {
            var serviceArgumentDefinition = serviceArgumentDefinitions[i];
            if (!serviceArgumentDefinition.IsGenericType)
            {
                continue;
            }

            var nestedServiceArguments = serviceArguments[i].GenericTypeArguments;
            if (nestedServiceArguments.Length == 0)
            {
                continue;
            }

            var nestedMatch = TryFindServiceArgumentForImplementationArgumentDefinition(
                implementationGenericArgumentDefinition, serviceArgumentDefinition.GenericTypeArguments, nestedServiceArguments);
            if (nestedMatch is not null)
            {
                return nestedMatch;
            }
        }

        return null;
    }

    private static bool IsCompatibleGenericClassDefinition(Type implementor, Type serviceType)
    {
        return implementor == serviceType || (implementor.IsGenericType && implementor.GetGenericTypeDefinition() == serviceType);
    }
}
