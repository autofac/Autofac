// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Diagnostics;
using Autofac.Util;

namespace Autofac.Core.Registration;

/// <summary>
/// Keeps track of the status of registered services.
/// </summary>
internal class DefaultRegisteredServicesTracker : Disposable, IRegisteredServicesTracker
{
    private static readonly Func<Service, ServiceRegistrationInfo> _regInfoFactory = srv => new ServiceRegistrationInfo(srv);

    private readonly Func<Service, IEnumerable<ServiceRegistration>> _registrationAccessor;

    /// <summary>
    /// Keeps track of the status of registered services.
    /// </summary>
    private readonly ConcurrentDictionary<Service, ServiceRegistrationInfo> _serviceInfo = new();

    /// <summary>
    /// External registration sources.
    /// </summary>
    private readonly Stack<IRegistrationSource> _dynamicRegistrationSources = new();

    /// <summary>
    /// All registrations.
    /// </summary>
    private readonly ConcurrentQueue<IComponentRegistration> _registrations = new();

    private readonly List<IServiceMiddlewareSource> _servicePipelineSources = new();

    [SuppressMessage(
        "CodeQuality",
        "IDE0052:Remove unread private members",
        Justification = "Intentionally holding a reference to the reflection cache in the tracker to keep the shared instance 'alive'.")]
    private readonly ReflectionCacheSet _capturedReflectionCache;

    private Dictionary<Service, ServiceRegistrationInfo>? _ephemeralServiceInfo;
    private bool _trackerPopulationComplete;

    /// <summary>
    /// Implementations a source produced for a service that had not yet drained that source, held
    /// until it does so the source's queue position rather than the order services happened to be
    /// resolved in decides the default.
    /// </summary>
    /// <remarks>
    /// Only a component exposing more than one service can populate this, which is rare, so the
    /// field stays <see langword="null"/> for most containers, where checking it is a field read.
    /// </remarks>
    private ConcurrentDictionary<(ServiceRegistrationInfo Info, IRegistrationSource Source), List<IComponentRegistration>>? _deferredSourceImplementations;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultRegisteredServicesTracker"/> class.
    /// </summary>
    public DefaultRegisteredServicesTracker()
    {
        _registrationAccessor = ServiceRegistrationsFor;

        // Hold a reference to the reflection cache here so the current instance stays
        // 'active' for the lifetime of the tracker (and therefore the container build + container).
        _capturedReflectionCache = ReflectionCacheSet.Shared;
    }

    /// <summary>
    /// Fired whenever a component is registered - either explicitly or via a
    /// <see cref="IRegistrationSource"/>.
    /// </summary>
    public event EventHandler<IComponentRegistration>? Registered;

    /// <summary>
    /// Fired when an <see cref="IRegistrationSource"/> is added to the registry.
    /// </summary>
    public event EventHandler<IRegistrationSource>? RegistrationSourceAdded;

    /// <inheritdoc />
    public IEnumerable<IComponentRegistration> Registrations
    {
        get
        {
            return _registrations;
        }
    }

    /// <inheritdoc />
    public IEnumerable<IRegistrationSource> Sources
    {
        get
        {
            return _dynamicRegistrationSources;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IServiceMiddlewareSource> ServiceMiddlewareSources => _servicePipelineSources;

    /// <inheritdoc/>
    public void AddServiceMiddleware(Service service, IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
    {
        var info = GetServiceInfo(service);

        info.UseServiceMiddleware(middleware, insertionMode);
    }

    /// <inheritdoc />
    public virtual void AddRegistration(IComponentRegistration registration, bool preserveDefaults, IRegistrationSource? originatingSource = null)
    {
        var originatedFromDynamicSource = originatingSource is not null;

        foreach (var service in registration.Services)
        {
            var info = GetServiceInfo(service);

            // We are in an ephemeral initialization; use the ephemeral set.
            if (_ephemeralServiceInfo is not null)
            {
                info = GetEphemeralServiceInfo(_ephemeralServiceInfo, service, info);
            }

            // A service this component exposes may have had the implementation deferred because it
            // has not drained this source yet, in which case it must not take it now - that would
            // place it ahead of the higher-priority sources still to be queried. Only a
            // multi-service component can defer, so the field is null for most containers.
            if (_deferredSourceImplementations is not null &&
                originatingSource is not null &&
                WasDeferred(info, originatingSource, registration))
            {
                continue;
            }

            info.AddImplementation(registration, preserveDefaults, originatedFromDynamicSource);
        }

        if (_ephemeralServiceInfo is null)
        {
            // Only when we are keeping the populated service information will we store registrations and
            // build pipelines for them.
            // The Registrations collection is only available to consumers once the tracker is contained with a ContainerRegistry
            // and the Complete method has been called.
            _registrations.Enqueue(registration);
            var handler = Registered;
            handler?.Invoke(this, registration);

            if (originatedFromDynamicSource)
            {
                registration.BuildResolvePipeline(this);
            }
        }
    }

    /// <inheritdoc />
    public void AddRegistrationSource(IRegistrationSource source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        _dynamicRegistrationSources.Push(source);

        var handler = RegistrationSourceAdded;
        handler?.Invoke(this, source);
    }

    /// <inheritdoc/>
    public void AddServiceMiddlewareSource(IServiceMiddlewareSource serviceMiddlewareSource)
    {
        if (serviceMiddlewareSource is null)
        {
            throw new ArgumentNullException(nameof(serviceMiddlewareSource));
        }

        _servicePipelineSources.Add(serviceMiddlewareSource);
    }

    /// <inheritdoc/>
    public IEnumerable<IResolveMiddleware> ServiceMiddlewareFor(Service service)
    {
        var info = GetInitializedServiceInfo(service);
        return info.ServiceMiddleware;
    }

    /// <inheritdoc />
    public bool TryGetRegistration(Service service, [NotNullWhen(returnValue: true)] out IComponentRegistration? registration)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var info = GetInitializedServiceInfo(service);
        return info.TryGetRegistration(out registration);
    }

    /// <inheritdoc/>
    public bool TryGetServiceRegistration(Service service, out ServiceRegistration serviceRegistration)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var info = GetInitializedServiceInfo(service);

        if (info.TryGetRegistration(out var registration))
        {
            serviceRegistration = new ServiceRegistration(info.ServicePipeline, registration);
            return true;
        }

        serviceRegistration = default;
        return false;
    }

    /// <inheritdoc />
    public bool IsRegistered(Service service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        return GetInitializedServiceInfo(service).IsRegistered;
    }

    /// <inheritdoc />
    public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var info = GetInitializedServiceInfo(service);
        return info.Implementations.ToList();
    }

    /// <inheritdoc/>
    public IEnumerable<ServiceRegistration> ServiceRegistrationsFor(Service service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        var info = GetInitializedServiceInfo(service);

        var list = new List<ServiceRegistration>();

        foreach (var implementation in info.Implementations)
        {
            list.Add(new ServiceRegistration(info.ServicePipeline, implementation));
        }

        return list;
    }

    /// <inheritdoc/>
    public void Complete()
    {
        _trackerPopulationComplete = true;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        foreach (var registration in _registrations)
        {
            registration.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        foreach (var registration in _registrations)
        {
            await registration.DisposeAsync().ConfigureAwait(false);
        }

        // Do not call the base, otherwise the standard Dispose will fire.
    }

    /// <summary>
    /// Acquires the service info lock and records wait time when metrics are enabled.
    /// </summary>
    /// <param name="info">The service info lock target.</param>
    /// <param name="instrumentationService">Optional detail about the service for metrics.</param>
    /// <param name="lockTaken">Tracks whether the lock was acquired.</param>
    private static void EnterServiceInfoLock(ServiceRegistrationInfo info, string? instrumentationService, ref bool lockTaken)
    {
        if (AutofacMetrics.MetricsEnabled)
        {
            var wait = ValueStopwatch.StartNew();
            Monitor.Enter(info, ref lockTaken);
            AutofacMetrics.RecordLockContention("Service", instrumentationService, wait.GetElapsedTime());
        }
        else
        {
            Monitor.Enter(info, ref lockTaken);
        }
    }

    /// <summary>
    /// Gets or creates an ephemeral service info entry used during pre-complete initialization.
    /// </summary>
    /// <param name="ephemeralSet">The ephemeral map for this initialization pass.</param>
    /// <param name="service">The service key.</param>
    /// <param name="info">The baseline service info to clone if needed.</param>
    /// <returns>An ephemeral service info entry for the service.</returns>
    private static ServiceRegistrationInfo GetEphemeralServiceInfo(Dictionary<Service, ServiceRegistrationInfo> ephemeralSet, Service service, ServiceRegistrationInfo info)
    {
        if (ephemeralSet.TryGetValue(service, out var ephemeral))
        {
            return ephemeral;
        }

        var newCopy = info.CloneUninitialized();

        ephemeralSet.Add(service, newCopy);

        return newCopy;
    }

    /// <summary>
    /// Unwraps scope-isolated services and notes when isolation applies.
    /// </summary>
    /// <param name="service">The service to inspect.</param>
    /// <param name="isScopeIsolatedService">Set to <see langword="true"/> when the service is scope isolated.</param>
    /// <returns>The inner service to process.</returns>
    private static Service ResolveScopeIsolation(Service service, ref bool isScopeIsolatedService)
    {
        if (service is ScopeIsolatedService scopeIsolatedService)
        {
            // This is an isolated service query; use the wrapped service instead and
            // remember that fact for later.
            isScopeIsolatedService = true;
            return scopeIsolatedService.Service;
        }

        return service;
    }

    /// <summary>
    /// Applies the implementations a source produced for this service earlier, while a different
    /// service sharing those components was being initialized, instead of querying the source again
    /// and creating duplicate components.
    /// </summary>
    /// <param name="info">The service info being populated.</param>
    /// <param name="source">The source about to be queried.</param>
    /// <returns><see langword="true"/> when the source had already run and its implementations were applied.</returns>
    private bool TryApplyDeferredSourceImplementations(ServiceRegistrationInfo info, IRegistrationSource source)
    {
        if (!_deferredSourceImplementations!.TryRemove((info, source), out var deferred))
        {
            return false;
        }

        lock (deferred)
        {
            foreach (var provided in deferred)
            {
                info.AddImplementation(provided, preserveDefaults: true, originatedFromSource: true);
            }
        }

        return true;
    }

    /// <summary>
    /// Holds an implementation for a service that has not drained the producing source yet, so it
    /// can be applied in source order once that service is initialized.
    /// </summary>
    /// <param name="info">The service info the implementation is being held for.</param>
    /// <param name="source">The source that produced the registration.</param>
    /// <param name="registration">The component registration.</param>
    private void DeferSourceImplementation(ServiceRegistrationInfo info, IRegistrationSource source, IComponentRegistration registration)
    {
        var deferred = _deferredSourceImplementations;
        if (deferred is null)
        {
            deferred = new ConcurrentDictionary<(ServiceRegistrationInfo, IRegistrationSource), List<IComponentRegistration>>();
            deferred = Interlocked.CompareExchange(ref _deferredSourceImplementations, deferred, null) ?? deferred;
        }

        // The list is shared, and two services initializing concurrently can both hold an
        // implementation against the same key, so guard it rather than mutating inside an
        // AddOrUpdate delegate that may run more than once.
        var held = deferred.GetOrAdd((info, source), static _ => new List<IComponentRegistration>());

        lock (held)
        {
            held.Add(registration);
        }
    }

    /// <summary>
    /// Determines whether an implementation is being held for a service rather than applied to it.
    /// </summary>
    /// <param name="info">The service info to check.</param>
    /// <param name="source">The source that produced the registration.</param>
    /// <param name="registration">The component registration.</param>
    /// <returns><see langword="true"/> when the implementation is being held.</returns>
    private bool WasDeferred(ServiceRegistrationInfo info, IRegistrationSource source, IComponentRegistration registration)
    {
        if (!_deferredSourceImplementations!.TryGetValue((info, source), out var deferred))
        {
            return false;
        }

        lock (deferred)
        {
            return deferred.Contains(registration);
        }
    }

    /// <summary>
    /// Ensures the service info is initialized and returns it.
    /// </summary>
    /// <param name="service">The service being queried.</param>
    /// <returns>The initialized service info.</returns>
    private ServiceRegistrationInfo GetInitializedServiceInfo(Service service)
    {
        var createdEphemeralSet = false;
        var isScopeIsolatedService = false;

        service = ResolveScopeIsolation(service, ref isScopeIsolatedService);

        var info = GetServiceInfo(service);
        var instrumentationService = AutofacMetrics.MetricsEnabled ? service.ToString() : null;
        if (info.IsInitialized)
        {
            return info;
        }

        info = GetServiceInfoForInitialization(service, info, ref createdEphemeralSet);

        var succeeded = false;
        var lockTaken = false;
        try
        {
            EnterServiceInfoLock(info, instrumentationService, ref lockTaken);

            if (info.IsInitialized)
            {
                return info;
            }

            // PopulateServiceInfo increments InitializationDepth; the decrement is paired in finally.
            succeeded = PopulateServiceInfo(service, info, isScopeIsolatedService);
        }
        finally
        {
            info.InitializationDepth--;

            if (info.InitializationDepth == 0)
            {
                if (succeeded)
                {
                    info.CompleteInitialization();
                }

                if (isScopeIsolatedService && (!succeeded || (!info.IsRegistered && !info.HasCustomServiceMiddleware)))
                {
                    // No registrations or custom middleware was found for this service, and this service enquiry is marked as "isolated",
                    // meaning that we shouldn't remember any info for it if it has no registrations.
                    _serviceInfo.TryRemove(service, out _);
                }
            }

            if (lockTaken)
            {
                Monitor.Exit(info);
            }

            // This method was the entry point to an ephemeral initialization pass.
            // Discard the temporary map so later calls start with a clean slate.
            if (createdEphemeralSet)
            {
                _ephemeralServiceInfo?.Clear();
                _ephemeralServiceInfo = null;
            }
        }

        return info;
    }

    /// <summary>
    /// Returns the appropriate service info for initialization, swapping to an ephemeral copy when needed.
    /// </summary>
    /// <param name="service">The service being queried.</param>
    /// <param name="info">The current service info.</param>
    /// <param name="createdEphemeralSet">Set to <see langword="true"/> when a new ephemeral set is created.</param>
    /// <returns>The service info to use for initialization.</returns>
    private ServiceRegistrationInfo GetServiceInfoForInitialization(Service service, ServiceRegistrationInfo info, ref bool createdEphemeralSet)
    {
        if (!_trackerPopulationComplete)
        {
            // We need an ephemeral set for this pre-complete initialization.
            if (_ephemeralServiceInfo is null)
            {
                _ephemeralServiceInfo = new Dictionary<Service, ServiceRegistrationInfo>();
                createdEphemeralSet = true;
            }

            info = GetEphemeralServiceInfo(_ephemeralServiceInfo, service, info);
        }

        return info;
    }

    /// <summary>
    /// Populates service info by querying registration sources and adding derived registrations.
    /// </summary>
    /// <param name="service">The service being initialized.</param>
    /// <param name="info">The service info to populate.</param>
    /// <param name="isScopeIsolatedService"><see langword="true"/> when per-scope sources should be skipped.</param>
    /// <returns><see langword="true"/> when initialization completes.</returns>
    private bool PopulateServiceInfo(Service service, ServiceRegistrationInfo info, bool isScopeIsolatedService)
    {
        if (!info.IsInitializing)
        {
            BeginServiceInfoInitialization(service, info, _dynamicRegistrationSources);
        }

        info.InitializationDepth++;

        // Drain sources in-order; registrations can enqueue additional sources.
        while (info.HasSourcesToQuery)
        {
            var next = info.DequeueNextSource();

            // Do not query per-scope registration sources for isolated services.
            if (isScopeIsolatedService && next is IPerScopeRegistrationSource)
            {
                continue;
            }

            // Null unless some multi-service component has deferred an implementation, so this is a
            // single field read for most containers.
            if (_deferredSourceImplementations is not null && TryApplyDeferredSourceImplementations(info, next))
            {
                continue;
            }

            foreach (var provided in next.RegistrationsFor(service, _registrationAccessor))
            {
                PopulateAdditionalServicesForProvidedRegistration(service, info, next, provided);
                AddRegistration(
                    provided,
                    preserveDefaults: true,
                    originatingSource: next);
            }
        }

        return true;
    }

    private void PopulateAdditionalServicesForProvidedRegistration(
        Service service,
        ServiceRegistrationInfo info,
        IRegistrationSource source,
        IComponentRegistration provided)
    {
        // This ensures that multiple services provided by the same
        // component share a single component (we don't re-query for them)
        foreach (var additionalService in provided.Services)
        {
            var additionalInfo = GetServiceInfo(additionalService);
            if (additionalInfo.IsInitialized || additionalInfo == info)
            {
                continue;
            }

            additionalInfo = UseEphemeralAdditionalInfoIfNeeded(service, info, additionalInfo);

            // Start the additional service's queue so this source keeps its place in it. The source
            // is not removed: it is consumed in order when the additional service is initialized.
            if (!additionalInfo.IsInitializing)
            {
                BeginServiceInfoInitialization(additionalService, additionalInfo, _dynamicRegistrationSources);
            }

            // Hold the implementation rather than letting the service take it now, which would put
            // it ahead of the higher-priority sources the service still has to query.
            if (additionalInfo.IsSourceQueued(source))
            {
                DeferSourceImplementation(additionalInfo, source, provided);
            }
        }
    }

    private ServiceRegistrationInfo UseEphemeralAdditionalInfoIfNeeded(
        Service service,
        ServiceRegistrationInfo info,
        ServiceRegistrationInfo additionalInfo)
    {
        if (_ephemeralServiceInfo is null)
        {
            return additionalInfo;
        }

        // Use ephemeral info for additional services.
        return GetEphemeralServiceInfo(_ephemeralServiceInfo, service, info);
    }

    /// <summary>
    /// Seeds service info with middleware and registration sources.
    /// </summary>
    /// <param name="service">The service being initialized.</param>
    /// <param name="info">The service info to update.</param>
    /// <param name="registrationSources">Sources to query for registrations.</param>
    private void BeginServiceInfoInitialization(Service service, ServiceRegistrationInfo info, IEnumerable<IRegistrationSource> registrationSources)
    {
        // Add any additional service pipeline configuration from external sources.
        foreach (var servicePipelineSource in _servicePipelineSources)
        {
            servicePipelineSource.ProvideMiddleware(service, this, info);
        }

        info.BeginInitialization(registrationSources);
    }

    /// <summary>
    /// Gets or creates the service info entry for a service key.
    /// </summary>
    /// <param name="service">The service key.</param>
    /// <returns>The service info entry.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ServiceRegistrationInfo GetServiceInfo(Service service)
    {
        return _serviceInfo.GetOrAdd(service, _regInfoFactory);
    }
}
