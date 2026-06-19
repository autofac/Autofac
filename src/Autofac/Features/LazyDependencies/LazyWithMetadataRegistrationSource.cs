// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Metadata;
using Autofac.Util;
using Autofac.Util.Cache;

namespace Autofac.Features.LazyDependencies;

/// <summary>
/// Support the <c>System.Lazy&lt;T, TMetadata&gt;</c>
/// types automatically whenever type T is registered with the container.
/// Metadata values come from the component registration's metadata.
/// When a dependency of a lazy type is used, the instantiation of the underlying
/// component will be delayed until the Value property is first accessed.
/// </summary>
internal class LazyWithMetadataRegistrationSource : IRegistrationSource
{
    private const string ReflectionCacheName = $"{nameof(LazyWithMetadataRegistrationSource)}.Cache";

    private static readonly MethodInfo _createLazyRegistrationMethod = typeof(LazyWithMetadataRegistrationSource).GetDeclaredMethod(nameof(CreateLazyRegistration));

    private delegate IComponentRegistration RegistrationCreator(Service providedService, Service valueService, ServiceRegistration registrationResolveInfo);

    /// <inheritdoc/>
    public bool IsAdapterForIndividualComponents => true;

    /// <inheritdoc/>
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "This source is registered for every container but only builds the Lazy<T, TMetadata> registration via MakeGenericMethod when a consumer actually resolves that relationship. The closed-type resolve path never reaches this. Consumers that resolve Lazy<T, TMetadata> over value-type arguments take on the dynamic-code requirement.")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2060:MakeGenericMethod",
        Justification = "The generic arguments are the relationship's value and metadata types supplied by the consumer at resolve time; preserving them is the responsibility of the consumer that resolves the relationship.")]
    public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (registrationAccessor == null)
        {
            throw new ArgumentNullException(nameof(registrationAccessor));
        }

        var lazyType = typeof(Lazy<,>);
        if (service is not IServiceWithType swt || !swt.ServiceType.IsGenericTypeDefinedBy(lazyType))
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var genericTypeArguments = swt.ServiceType.GenericTypeArguments;
        var valueType = genericTypeArguments[0];
        var metaType = genericTypeArguments[1];

        if (!metaType.IsClass)
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var valueService = swt.ChangeType(valueType);

        // Use the non-internal form here because the dictionary value is a type internal to
        // this source.
        var methodCache = ReflectionCacheSet.Shared.GetOrCreateCache<ReflectionCacheTupleDictionary<Type, RegistrationCreator>>(ReflectionCacheName);

        var registrationCreator = methodCache.GetOrAdd((valueType, metaType), types =>
        {
            return _createLazyRegistrationMethod.MakeGenericMethod(types.Item1, types.Item2).CreateDelegate<RegistrationCreator>(null);
        });

        return registrationAccessor(valueService)
            .Select(v => registrationCreator(service, valueService, v));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return LazyWithMetadataRegistrationSourceResources.LazyWithMetadataRegistrationSourceDescription;
    }

    [RequiresDynamicCode("Lazy<T, TMetadata> builds a strongly-typed metadata view at runtime via expression compilation; only reached when a consumer resolves the relationship.")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2072:UnrecognizedReflectionPattern",
        Justification = "lazyType is the closed Lazy<T, TMetadata> the consumer requested. Activator.CreateInstance invokes its (T factory, metadata) constructor, which is intrinsic to Lazy<,> and always present; the consumer that resolves the relationship is responsible for that type.")]
    private static IComponentRegistration CreateLazyRegistration<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TMeta>(Service providedService, Service valueService, ServiceRegistration registrationResolveInfo)
    {
        var metadataProvider = MetadataViewProvider.GetMetadataViewProvider<TMeta>();

        var rb = RegistrationBuilder.ForDelegate(
            (c, p) =>
            {
                var context = c.Resolve<IComponentContext>();
                var lazyType = ((IServiceWithType)providedService).ServiceType;
                var request = new ResolveRequest(valueService, registrationResolveInfo, p);
                var valueFactory = new Func<T>(() => (T)context.ResolveComponent(request));
                var metadata = metadataProvider(registrationResolveInfo.Registration.Target.Metadata);
                return Activator.CreateInstance(lazyType, valueFactory, metadata)!;
            })
            .As(providedService)
            .Targeting(registrationResolveInfo.Registration);

        return rb.CreateRegistration();
    }
}
