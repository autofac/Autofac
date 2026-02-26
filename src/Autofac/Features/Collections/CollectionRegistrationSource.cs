// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Linq.Expressions;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Diagnostics;
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
                var registrationTuples = isAnyKeyQuery
                    ? GetAllSpecificKeyedRegistrations(c.ComponentRegistry, elementType)
                        .ConvertAll(static tuple => ((Service)tuple.KeyedService, tuple.Registration))
                    : BuildStandardRegistrationList(c.ComponentRegistry, elementTypeService);

                var collectionKind = isAnyKeyQuery ? "any-keyed" : "standard";
                var collectionDetail = isAnyKeyQuery
                    ? elementType.FullName ?? elementType.Name
                    : elementTypeService.ToString() ?? elementTypeService.GetType().Name;
                return BuildCollection(c, factory, registrationTuples, p, collectionKind, collectionDetail);
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

    /// <summary>
    /// When the query is for "any keyed" enumerable, we need to find all the
    /// specific keyed registrations and return them.
    /// </summary>
    /// <param name="registry">
    /// The registry to search for registrations. We need to search the entire
    /// registry because "any keyed" could match any specific key.
    /// </param>
    /// <param name="elementType">
    /// The element type of the enumerable being resolved. We need this to
    /// filter the registry down to only the relevant registrations.
    /// </param>
    /// <returns>
    /// A list of tuples containing the specific keyed service and the
    /// registration for each matching registration. We return the specific
    /// keyed service so that we can issue resolve requests that still know the
    /// original key.
    /// </returns>
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

    private static List<(Service Service, ServiceRegistration Registration)> BuildStandardRegistrationList(IComponentRegistry registry, Service elementTypeService)
    {
        return registry
            .ServiceRegistrationsFor(elementTypeService)
            .Where(cr => !cr.Registration.Options.HasOption(RegistrationOptions.ExcludeFromCollections))
            .OrderBy(cr => cr.Registration.GetRegistrationOrder())
            .Select(cr => ((Service)elementTypeService, cr))
            .ToList();
    }

    private static IList BuildCollection(
        IComponentContext context,
        Func<int, IList> factory,
        List<(Service Service, ServiceRegistration Registration)> registrations,
        IEnumerable<Parameter> parameters,
        string collectionKind,
        string collectionDetail)
    {
        var recordMetrics = AutofacMetrics.MetricsEnabled;
        ValueStopwatch instrumentationTimer = default;
        if (recordMetrics)
        {
            instrumentationTimer = ValueStopwatch.StartNew();
        }

        var output = factory(registrations.Count);
        var isFixedSize = output.IsFixedSize;

        for (var i = 0; i < registrations.Count; i++)
        {
            var (service, registration) = registrations[i];
            var resolveRequest = new ResolveRequest(service, registration, parameters);
            var component = context.ResolveComponent(resolveRequest);

            if (isFixedSize)
            {
                output[i] = component;
            }
            else
            {
                output.Add(component);
            }
        }

        if (recordMetrics)
        {
            AutofacMetrics.RecordCollectionBuild(
                kind: collectionKind,
                detail: collectionDetail,
                itemCount: registrations.Count,
                elapsedTicks: instrumentationTimer.ElapsedTicks);
        }

        return output;
    }
}
