// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core.Activators.Reflection;
using Autofac.Features.GeneratedFactories;
using Autofac.Util;
using Autofac.Util.Cache;

namespace Autofac.Core;

/// <summary>
/// Defines known, internal-only caches that are used in relatively hot paths, so we want to
/// avoid the additional dictionary lookup in <see cref="ReflectionCacheSet.GetOrCreateCache(string)"/>.
/// </summary>
internal class InternalReflectionCaches
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InternalReflectionCaches"/> class.
    /// </summary>
    /// <param name="set">The cache set used to retrieve the required caches.</param>
    public InternalReflectionCaches(ReflectionCacheSet set)
    {
        AssemblyScanAllowedTypes = set.GetOrCreateCache(nameof(AssemblyScanAllowedTypes), _ => new ReflectionCacheAssemblyDictionary<Assembly, IEnumerable<Type>>
        {
            Usage = ReflectionCacheUsage.Registration,
        });

        ModuleOverridesAttachToComponentRegistration = set.GetOrCreateCache(nameof(ModuleOverridesAttachToComponentRegistration), _ => new ReflectionCacheDictionary<Type, bool>
        {
            Usage = ReflectionCacheUsage.Registration,
        });

        ModuleOverridesAttachToRegistrationSource = set.GetOrCreateCache(nameof(ModuleOverridesAttachToRegistrationSource), _ => new ReflectionCacheDictionary<Type, bool>
        {
            Usage = ReflectionCacheUsage.Registration,
        });

        IsGenericEnumerableInterface = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>(nameof(IsGenericEnumerableInterface));
        IsGenericListOrCollectionInterfaceType = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>(nameof(IsGenericListOrCollectionInterfaceType));
        IsGenericTypeDefinedBy = set.GetOrCreateCache<ReflectionCacheTupleDictionary<Type, bool>>(nameof(IsGenericTypeDefinedBy));
        IsGenericTypeContainingType = set.GetOrCreateCache<ReflectionCacheTupleDictionary<Type, bool>>(nameof(IsGenericTypeContainingType));
        ConstructorBinderFactory = set.GetOrCreateCache<ReflectionCacheDictionary<ConstructorInfo, Func<object?[], object>>>(nameof(ConstructorBinderFactory));
        AutowiringPropertySetters = set.GetOrCreateCache<ReflectionCacheDictionary<PropertyInfo, Action<object, object?>>>(nameof(AutowiringPropertySetters));
        AutowiringInjectableProperties = set.GetOrCreateCache<ReflectionCacheDictionary<Type, IReadOnlyList<PropertyInfo>>>(nameof(AutowiringInjectableProperties));
        DefaultPublicConstructors = set.GetOrCreateCache<ReflectionCacheDictionary<Type, ConstructorInfo[]>>(nameof(DefaultPublicConstructors));
        GenericTypeDefinitionByType = set.GetOrCreateCache<ReflectionCacheDictionary<Type, Type>>(nameof(GenericTypeDefinitionByType));
        HasRequiredMemberAttribute = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>(nameof(HasRequiredMemberAttribute));
        ServiceKeyParameterAttributes = set.GetOrCreateCache<ReflectionCacheParameterDictionary<bool>>(nameof(ServiceKeyParameterAttributes));
        ServiceKeyPropertyAttributes = set.GetOrCreateCache<ReflectionCacheDictionary<PropertyInfo, bool>>(nameof(ServiceKeyPropertyAttributes));
        ServiceKeyUsageByType = set.GetOrCreateCache<ReflectionCacheDictionary<Type, bool>>(nameof(ServiceKeyUsageByType));

        GeneratedFactoryServiceOnlyGenerators = set.GetOrCreateCache(
            nameof(GeneratedFactoryServiceOnlyGenerators),
            _ => new ReflectionCacheTypeKeyedDictionary<ParameterMapping, Func<Service, IComponentContext, IEnumerable<Parameter>, Delegate>>
            {
                Usage = ReflectionCacheUsage.All,
            });

        GeneratedFactoryServiceRegistrationGenerators = set.GetOrCreateCache(
            nameof(GeneratedFactoryServiceRegistrationGenerators),
            _ => new ReflectionCacheTypeKeyedDictionary<ParameterMapping, Func<Service, ServiceRegistration, IComponentContext, IEnumerable<Parameter>, Delegate>>
            {
                Usage = ReflectionCacheUsage.All,
            });
    }

    /// <summary>
    /// Gets the cache used by <see cref="Features.Scanning.AssemblyExtensions.GetPermittedTypesForAssemblyScanning"/>.
    /// </summary>
    public ReflectionCacheAssemblyDictionary<Assembly, IEnumerable<Type>> AssemblyScanAllowedTypes
    {
        get;
    }

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericEnumerableInterfaceType"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> IsGenericEnumerableInterface
    {
        get;
    }

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericListOrCollectionInterfaceType"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> IsGenericListOrCollectionInterfaceType
    {
        get;
    }

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericTypeDefinedBy"/>.
    /// </summary>
    public ReflectionCacheTupleDictionary<Type, bool> IsGenericTypeDefinedBy
    {
        get;
    }

    /// <summary>
    /// Gets the cache used by <see cref="InternalTypeExtensions.IsGenericTypeContainingType"/>.
    /// </summary>
    public ReflectionCacheTupleDictionary<Type, bool> IsGenericTypeContainingType
    {
        get;
    }

    /// <summary>
    /// Gets the cache used by <see cref="ConstructorBinder"/>.
    /// </summary>
    public ReflectionCacheDictionary<ConstructorInfo, Func<object?[], object>> ConstructorBinderFactory
    {
        get;
    }

    /// <summary>
    /// Gets a cache used by <see cref="AutowiringPropertyInjector.InjectProperties"/>.
    /// </summary>
    public ReflectionCacheDictionary<PropertyInfo, Action<object, object?>> AutowiringPropertySetters
    {
        get;
    }

    /// <summary>
    /// Gets a cache used by <see cref="AutowiringPropertyInjector.InjectProperties"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, IReadOnlyList<PropertyInfo>> AutowiringInjectableProperties
    {
        get;
    }

    /// <summary>
    /// Gets a cache used by <see cref="DefaultConstructorFinder"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, ConstructorInfo[]> DefaultPublicConstructors
    {
        get;
    }

    /// <summary>
    /// Gets a cache of memoized <see cref="Type.GetGenericTypeDefinition"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, Type> GenericTypeDefinitionByType
    {
        get;
    }

    /// <summary>
    /// Gets a cache used by <see cref="ReflectionActivator"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> HasRequiredMemberAttribute
    {
        get;
    }

    /// <summary>
    /// Gets a cache used to track <see cref="ServiceKeyAttribute"/> usage on parameters.
    /// </summary>
    public ReflectionCacheParameterDictionary<bool> ServiceKeyParameterAttributes
    {
        get;
    }

    /// <summary>
    /// Gets a cache used to track <see cref="ServiceKeyAttribute"/> usage on properties.
    /// </summary>
    public ReflectionCacheDictionary<PropertyInfo, bool> ServiceKeyPropertyAttributes
    {
        get;
    }

    /// <summary>
    /// Gets a cache used to determine if a type uses <see cref="ServiceKeyAttribute"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> ServiceKeyUsageByType
    {
        get;
    }

    /// <summary>
    /// Gets a cache used by <see cref="Module"/> to determine whether a concrete module type
    /// overrides <see cref="Module.AttachToComponentRegistration"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> ModuleOverridesAttachToComponentRegistration
    {
        get;
    }

    /// <summary>
    /// Gets a cache used by <see cref="Module"/> to determine whether a concrete module type
    /// overrides <see cref="Module.AttachToRegistrationSource"/>.
    /// </summary>
    public ReflectionCacheDictionary<Type, bool> ModuleOverridesAttachToRegistrationSource
    {
        get;
    }

    /// <summary>
    /// Gets the cache of compiled factory-delegate generators for
    /// <see cref="Features.GeneratedFactories.FactoryGenerator"/> instances that resolve via
    /// <c>ResolveService</c>. Keyed on <c>(delegateType, effectiveParameterMapping)</c>.
    /// </summary>
    public ReflectionCacheTypeKeyedDictionary<ParameterMapping, Func<Service, IComponentContext, IEnumerable<Parameter>, Delegate>> GeneratedFactoryServiceOnlyGenerators
    {
        get;
    }

    /// <summary>
    /// Gets the cache of compiled factory-delegate generators for
    /// <see cref="Features.GeneratedFactories.FactoryGenerator"/> instances that resolve via
    /// <see cref="IComponentContext.ResolveComponent"/>. Keyed on <c>(delegateType, effectiveParameterMapping)</c>.
    /// </summary>
    public ReflectionCacheTypeKeyedDictionary<ParameterMapping, Func<Service, ServiceRegistration, IComponentContext, IEnumerable<Parameter>, Delegate>> GeneratedFactoryServiceRegistrationGenerators
    {
        get;
    }
}
