// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Keeps track of the status of registered services.
    /// </summary>
    internal class DefaultRegisteredServicesTracker : Disposable, IRegisteredServicesTracker
    {
        private static readonly Func<Service, ServiceRegistrationInfo> RegInfoFactory = srv => new ServiceRegistrationInfo(srv);

        private readonly Func<Service, IEnumerable<ServiceRegistration>> _registrationAccessor;

        /// <summary>
        /// Keeps track of the status of registered services.
        /// </summary>
        private readonly ConcurrentDictionary<Service, ServiceRegistrationInfo> _serviceInfo = new ConcurrentDictionary<Service, ServiceRegistrationInfo>();

        /// <summary>
        /// External registration sources.
        /// </summary>
        private readonly List<IRegistrationSource> _dynamicRegistrationSources = new List<IRegistrationSource>();

        /// <summary>
        /// All registrations.
        /// </summary>
        private readonly ConcurrentBag<IComponentRegistration> _registrations = new ConcurrentBag<IComponentRegistration>();

        private readonly List<IServiceMiddlewareSource> _servicePipelineSources = new List<IServiceMiddlewareSource>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRegisteredServicesTracker"/> class.
        /// </summary>
        public DefaultRegisteredServicesTracker()
        {
            _registrationAccessor = ServiceRegistrationsFor;
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
                return _registrations.ToList();
            }
        }

        /// <inheritdoc />
        public IEnumerable<IRegistrationSource> Sources
        {
            get
            {
                return _dynamicRegistrationSources.ToList();
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
        public virtual void AddRegistration(IComponentRegistration registration, bool preserveDefaults, bool originatedFromDynamicSource = false)
        {
            foreach (var service in registration.Services)
            {
                var info = GetServiceInfo(service);
                info.AddImplementation(registration, preserveDefaults, originatedFromDynamicSource);
            }

            _registrations.Add(registration);
            var handler = Registered;
            handler?.Invoke(this, registration);

            if (originatedFromDynamicSource)
            {
                registration.BuildResolvePipeline(this);
            }
        }

        /// <inheritdoc />
        public void AddRegistrationSource(IRegistrationSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            _dynamicRegistrationSources.Insert(0, source);

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
        public bool TryGetServiceRegistration(Service service, out ServiceRegistration serviceData)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            var info = GetInitializedServiceInfo(service);

            if (info.TryGetRegistration(out var registration))
            {
                serviceData = new ServiceRegistration(info.ServicePipeline, registration);
                return true;
            }

            serviceData = default;
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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            foreach (var registration in _registrations)
            {
                registration.Dispose();
            }

            base.Dispose(disposing);
        }

        private ServiceRegistrationInfo GetInitializedServiceInfo(Service service)
        {
            var info = GetServiceInfo(service);
            if (info.IsInitialized)
            {
                return info;
            }

            var succeeded = false;
            var lockTaken = false;
            try
            {
                Monitor.Enter(info, ref lockTaken);

                if (info.IsInitialized)
                {
                    return info;
                }

                if (!info.IsInitializing)
                {
                    BeginServiceInfoInitialization(service, info, _dynamicRegistrationSources);
                }

                info.InitializationDepth++;

                while (info.HasSourcesToQuery)
                {
                    var next = info.DequeueNextSource();
                    foreach (var provided in next.RegistrationsFor(service, _registrationAccessor))
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

                            if (!additionalInfo.IsInitializing)
                            {
                                BeginServiceInfoInitialization(additionalService, additionalInfo, ExcludeSource(_dynamicRegistrationSources, next));
                            }
                            else
                            {
                                additionalInfo.SkipSource(next);
                            }
                        }

                        AddRegistration(provided, true, true);
                    }
                }

                succeeded = true;
            }
            finally
            {
                info.InitializationDepth--;

                if (info.InitializationDepth == 0 && succeeded)
                {
                    info.CompleteInitialization();
                }

                if (lockTaken)
                {
                    Monitor.Exit(info);
                }
            }

            return info;
        }

        private void BeginServiceInfoInitialization(Service service, ServiceRegistrationInfo info, IEnumerable<IRegistrationSource> registrationSources)
        {
            // Add any additional service pipeline configuration from external sources.
            foreach (var servicePipelineSource in _servicePipelineSources)
            {
                servicePipelineSource.ProvideMiddleware(service, this, info);
            }

            info.BeginInitialization(registrationSources);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<IRegistrationSource> ExcludeSource(IEnumerable<IRegistrationSource> sources, IRegistrationSource exclude)
        {
            foreach (var item in sources)
            {
                if (item != exclude)
                {
                    yield return item;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ServiceRegistrationInfo GetServiceInfo(Service service)
        {
            return _serviceInfo.GetOrAdd(service, RegInfoFactory);
        }
    }
}
