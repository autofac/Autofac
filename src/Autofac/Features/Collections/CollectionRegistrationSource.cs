// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Linq.Expressions;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Features.Decorators;
using Autofac.Util;
using Autofac.Util.Cache;

namespace Autofac.Features.Collections;

/// <summary>
/// Registration source providing implicit collection/list/enumerable support.
/// </summary>
/// <remarks>
/// <para>
/// This registration source provides enumerable support to allow resolving
/// the set of all registered services of a given type.
/// </para>
/// <para>
/// What may not be immediately apparent is that it also means any time there
/// are no items of a particular type registered, it will always return an
/// empty set rather than <see langword="null" /> or throwing an exception.
/// This is by design.
/// </para>
/// <para>
/// Consider the [possibly majority] use case where you're resolving a set
/// of message handlers or event handlers from the container. If there aren't
/// any handlers, you want an empty set - not <see langword="null" /> or
/// an exception. It's valid to have no handlers registered.
/// </para>
/// <para>
/// This implicit support means other areas (like MVC support or manual
/// property injection) must take care to only request enumerable values they
/// expect to get something back for. In other words, "Don't ask the container
/// for something you don't expect to resolve".
/// </para>
/// </remarks>
internal class CollectionRegistrationSource : IRegistrationSource, IPerScopeRegistrationSource
{
    /// <inheritdoc/>
    public bool IsAdapterForIndividualComponents => false;

    /// <summary>
    /// Retrieve registrations for an unregistered service, to be used
    /// by the container.
    /// </summary>
    /// <param name="service">The service that was requested.</param>
    /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
    /// <returns>Registrations providing the service.</returns>
    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Activator lifetime controlled by registry.")]
    public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (registrationAccessor == null)
        {
            throw new ArgumentNullException(nameof(registrationAccessor));
        }

        if (service is not IServiceWithType swt || service is DecoratorService)
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var serviceType = swt.ServiceType;

        var factoryCache = ReflectionCacheSet.Shared.GetOrCreateCache<ReflectionCacheDictionary<Type, (Type? ElementType, Type? LimitType, Func<int, IList>? Factory)>>(nameof(CollectionRegistrationSource));

        var (elementType, limitType, factory) = factoryCache.GetOrAdd(serviceType, static serviceType =>
        {
            Type? elementType = null;
            Type? limitType = null;
            Func<int, IList>? factory = null;

            if (serviceType.IsGenericTypeDefinedBy(typeof(IEnumerable<>)))
            {
                elementType = serviceType.GenericTypeArguments[0];
                limitType = elementType.MakeArrayType();
                factory = GenerateArrayFactory(elementType);
            }
            else if (serviceType.IsArray)
            {
                // GetElementType always non-null if IsArray is true.
                elementType = serviceType.GetElementType()!;
                limitType = serviceType;
                factory = GenerateArrayFactory(elementType);
            }
            else if (serviceType.IsGenericListOrCollectionInterfaceType())
            {
                elementType = serviceType.GenericTypeArguments[0];
                limitType = typeof(List<>).MakeGenericType(elementType);
                factory = GenerateListFactory(elementType);
            }

            return (elementType, limitType, factory);
        });

        if (elementType == null || factory == null || limitType == null)
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var elementTypeService = swt.ChangeType(elementType);
        var isAnyKeyQuery = service is KeyedService keyedService && KeyedService.IsAnyKey(keyedService.ServiceKey);

        var activator = new DelegateActivator(
            limitType,
            (c, p) =>
            {
                if (isAnyKeyQuery)
                {
                    // AnyKey queries for collections return _all_ explicitly
                    // keyed services. (Services _registered_ with AnyKey are
                    // intentionally excluded, however, since they are
                    // effectively "default" registrations and would be
                    // duplicated in the output if included alongside their
                    // explicitly keyed counterparts.)
                    //
                    // We must resolve each element using the concrete keyed
                    // service so the KeyedServiceParameterInjector can flow
                    // that key into [ServiceKey] parameters.
                    var keyedRegistrations = GetAllSpecificKeyedRegistrations(c.ComponentRegistry, elementType);
                    var output = factory(keyedRegistrations.Count);
                    var isFixedSize = output.IsFixedSize;

                    for (var i = 0; i < keyedRegistrations.Count; i++)
                    {
                        var (keyedService, itemRegistration) = keyedRegistrations[i];
                        var resolveRequest = new ResolveRequest(keyedService, itemRegistration, p);
                        var component = c.ResolveComponent(resolveRequest);
                        if (isFixedSize)
                        {
                            output[i] = component;
                        }
                        else
                        {
                            output.Add(component);
                        }
                    }

                    return output;
                }

                var itemRegistrations = c.ComponentRegistry
                    .ServiceRegistrationsFor(elementTypeService)
                    .Where(cr => !cr.Registration.Options.HasOption(RegistrationOptions.ExcludeFromCollections))
                    .OrderBy(cr => cr.Registration.GetRegistrationOrder())
                    .ToList();

                var defaultOutput = factory(itemRegistrations.Count);
                var defaultOutputIsFixedSize = defaultOutput.IsFixedSize;

                for (var i = 0; i < itemRegistrations.Count; i++)
                {
                    var itemRegistration = itemRegistrations[i];
                    var resolveRequest = new ResolveRequest(elementTypeService, itemRegistration, p);
                    var component = c.ResolveComponent(resolveRequest);
                    if (defaultOutputIsFixedSize)
                    {
                        defaultOutput[i] = component;
                    }
                    else
                    {
                        defaultOutput.Add(component);
                    }
                }

                return defaultOutput;
            });

        var registration = new ComponentRegistration(
            Guid.NewGuid(),
            activator,
            CurrentScopeLifetime.Instance,
            isAnyKeyQuery ? InstanceSharing.Shared : InstanceSharing.None,
            isAnyKeyQuery ? InstanceOwnership.OwnedByLifetimeScope : InstanceOwnership.ExternallyOwned,
            new[] { service },
            new Dictionary<string, object?>());

        return new IComponentRegistration[] { registration };
    }

    /// <inheritdoc/>
    public override string ToString()
        => CollectionRegistrationSourceResources.CollectionRegistrationSourceDescription;

    private static Func<int, IList> GenerateListFactory(Type elementType)
    {
        var parameter = Expression.Parameter(typeof(int));
        var genericType = typeof(List<>).MakeGenericType(elementType);
        var constructor = genericType.GetMatchingConstructor(new[] { typeof(int) });

        // We know that List<> has the constructor we need.
        var newList = Expression.New(constructor!, parameter);
        return Expression.Lambda<Func<int, IList>>(newList, parameter).Compile();
    }

    private static Func<int, IList> GenerateArrayFactory(Type elementType)
    {
        var parameter = Expression.Parameter(typeof(int));
        var newArray = Expression.NewArrayBounds(elementType, parameter);
        return Expression.Lambda<Func<int, IList>>(newArray, parameter).Compile();
    }

    private static List<(KeyedService KeyedService, ServiceRegistration Registration)> GetAllSpecificKeyedRegistrations(IComponentRegistry registry, Type elementType)
    {
        var result = new List<(KeyedService, ServiceRegistration)>();
        var processedServices = new HashSet<KeyedService>();

        foreach (var registration in registry.Registrations)
        {
            if (registration.Metadata.ContainsKey(MetadataKeys.AnyKeyAdapter))
            {
                continue;
            }

            foreach (var keyed in registration.Services.OfType<KeyedService>())
            {
                if (keyed.ServiceType != elementType || KeyedService.IsAnyKey(keyed.ServiceKey))
                {
                    continue;
                }

                if (!processedServices.Add(keyed))
                {
                    continue;
                }

                var serviceRegistrations = registry
                    .ServiceRegistrationsFor(keyed)
                    .Where(cr =>
                        !cr.Registration.Options.HasOption(RegistrationOptions.ExcludeFromCollections) &&
                        !cr.Registration.Metadata.ContainsKey(MetadataKeys.AnyKeyAdapter));

                foreach (var serviceRegistration in serviceRegistrations)
                {
                    // Return both the keyed service and the registration so callers can issue
                    // resolve requests that still know the original key.
                    result.Add((keyed, serviceRegistration));
                }
            }
        }

        return result
            .OrderBy(tuple => tuple.Item2.Registration.GetRegistrationOrder())
            .ToList();
    }
}
