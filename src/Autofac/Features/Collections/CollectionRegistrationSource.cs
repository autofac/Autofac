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
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "The collection registration source is registered for every container but only constructs the List<T>/array element type via MakeGenericType/MakeArrayType when a consumer actually resolves IEnumerable<T>/IList<T>/T[]. The closed-type resolve path never reaches this. Consumers that resolve collections of value types take on the dynamic-code requirement.")]
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

        if (!TryGetCollectionBuildInfo(elementType, limitType, factory, out var buildInfo))
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var elementTypeService = swt.ChangeType(buildInfo.ElementType);
        var isAnyKeyQuery = service is KeyedService keyedService && KeyedService.IsAnyKey(keyedService.ServiceKey);

        var activator = new DelegateActivator(
            buildInfo.LimitType,
            (c, p) =>
            {
                var registrationTuples = isAnyKeyQuery
                    ? GetAllSpecificKeyedRegistrations(c.ComponentRegistry, buildInfo.ElementType)
                        .ConvertAll(static tuple => ((Service)tuple.KeyedService, tuple.Registration))
                    : BuildStandardRegistrationList(c.ComponentRegistry, elementTypeService);

                var (collectionKind, collectionDetail) = GetCollectionMetricsDetails(isAnyKeyQuery, buildInfo.ElementType, elementTypeService);

                return BuildCollection(c, buildInfo.Factory, registrationTuples, p, collectionKind, collectionDetail);
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

    private static bool TryGetCollectionBuildInfo(Type? elementType, Type? limitType, Func<int, IList>? factory, out (Type ElementType, Type LimitType, Func<int, IList> Factory) buildInfo)
    {
        if (elementType is null || limitType is null || factory is null)
        {
            buildInfo = default;
            return false;
        }

        buildInfo = (elementType, limitType, factory);
        return true;
    }

    private static (string? CollectionKind, string? CollectionDetail) GetCollectionMetricsDetails(bool isAnyKeyQuery, Type elementType, Service elementTypeService)
    {
        if (!AutofacMetrics.MetricsEnabled)
        {
            return (null, null);
        }

        // The collection kind and detail are only used in recording metrics.
        var collectionKind = isAnyKeyQuery ? "any-keyed" : "standard";
        var collectionDetail = isAnyKeyQuery
            ? elementType.FullName ?? elementType.Name
            : elementTypeService.ToString() ?? elementTypeService.GetType().Name;

        return (collectionKind, collectionDetail);
    }

    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "Only reached when a consumer resolves IList<T>/ICollection<T>; constructing the closed List<T> is the consumer's dynamic-code requirement, already surfaced at the resolve of the collection relationship.")]
    private static Func<int, IList> GenerateListFactory(Type elementType)
    {
        var parameter = Expression.Parameter(typeof(int));
        var genericType = typeof(List<>).MakeGenericType(elementType);
        var constructor = genericType.GetMatchingConstructor(new[] { typeof(int) });

        // We know that List<> has the constructor we need.
        var newList = Expression.New(constructor!, parameter);
        return Expression.Lambda<Func<int, IList>>(newList, parameter).Compile();
    }

    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "Only reached when a consumer resolves T[]/IEnumerable<T>; constructing the array type is the consumer's dynamic-code requirement, already surfaced at the resolve of the collection relationship.")]
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
            AppendSpecificKeyedRegistrations(registry, elementType, registration, processedServices, result);
        }

        result.Sort(static (a, b) => a.Item2.GetRegistrationOrder().CompareTo(b.Item2.GetRegistrationOrder()));
        return result;
    }

    private static void AppendSpecificKeyedRegistrations(
        IComponentRegistry registry,
        Type elementType,
        IComponentRegistration registration,
        HashSet<KeyedService> processedServices,
        List<(KeyedService KeyedService, ServiceRegistration Registration)> result)
    {
        if (registration.Metadata.ContainsKey(MetadataKeys.AnyKeyAdapter))
        {
            return;
        }

        foreach (var keyed in registration.Services.OfType<KeyedService>())
        {
            if (!IsSpecificKeyedElementService(keyed, elementType) || !processedServices.Add(keyed))
            {
                continue;
            }

            foreach (var serviceRegistration in registry.ServiceRegistrationsFor(keyed))
            {
                if (ShouldIncludeInSpecificKeyedCollection(serviceRegistration))
                {
                    result.Add((keyed, serviceRegistration));
                }
            }
        }
    }

    private static bool IsSpecificKeyedElementService(KeyedService keyed, Type elementType)
        => keyed.ServiceType == elementType && !KeyedService.IsAnyKey(keyed.ServiceKey);

    private static bool ShouldIncludeInSpecificKeyedCollection(ServiceRegistration serviceRegistration)
        => !serviceRegistration.Registration.Options.HasOption(RegistrationOptions.ExcludeFromCollections)
            && !serviceRegistration.Registration.Metadata.ContainsKey(MetadataKeys.AnyKeyAdapter);

    private static List<(Service Service, ServiceRegistration Registration)> BuildStandardRegistrationList(IComponentRegistry registry, Service elementTypeService)
    {
        var registrations = registry.ServiceRegistrationsFor(elementTypeService);
        var result = new List<(Service, ServiceRegistration)>();

        foreach (var cr in registrations)
        {
            if (!cr.Registration.Options.HasOption(RegistrationOptions.ExcludeFromCollections))
            {
                result.Add((elementTypeService, cr));
            }
        }

        result.Sort(static (a, b) => a.Item2.GetRegistrationOrder().CompareTo(b.Item2.GetRegistrationOrder()));
        return result;
    }

    private static IList BuildCollection(
        IComponentContext context,
        Func<int, IList> factory,
        List<(Service Service, ServiceRegistration Registration)> registrations,
        IEnumerable<Parameter> parameters,
        string? collectionKind,
        string? collectionDetail)
    {
        // Collection kind and detail will be null unless metrics are enabled.
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
                kind: collectionKind!,
                detail: collectionDetail,
                itemCount: registrations.Count,
                elapsed: instrumentationTimer.GetElapsedTime());
        }

        return output;
    }
}
