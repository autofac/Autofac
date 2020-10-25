// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac.Core.Pipeline;
using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Tracks the services known to the registry.
    /// </summary>
    internal class ServiceRegistrationInfo : IResolvePipelineBuilder
    {
        private volatile bool _isInitialized;

        private readonly Service _service;

        private IComponentRegistration? _fixedRegistration;

        /// <summary>
        ///  List of implicit default service implementations. Overriding default implementations are appended to the end,
        ///  so the enumeration should begin from the end too, and the most default implementation comes last.
        /// </summary>
        private readonly List<IComponentRegistration> _defaultImplementations = new List<IComponentRegistration>();

        /// <summary>
        ///  List of service implementations coming from sources. Sources have priority over preserve-default implementations.
        ///  Implementations from sources are enumerated in preserve-default order, so the most default implementation comes first.
        /// </summary>
        private List<IComponentRegistration>? _sourceImplementations;

        /// <summary>
        ///  List of explicit service implementations specified with the PreserveExistingDefaults option.
        ///  Enumerated in preserve-defaults order, so the most default implementation comes first.
        /// </summary>
        private List<IComponentRegistration>? _preserveDefaultImplementations;

        [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "The creator of the compponent registration is responsible for disposal.")]
        private IComponentRegistration? _defaultImplementation;

        /// <summary>
        /// Used for bookkeeping so that the same source is not queried twice (may be null).
        /// </summary>
        private Queue<IRegistrationSource>? _sourcesToQuery;

        private IResolvePipeline? _resolvePipeline;

        private IResolvePipelineBuilder? _customPipelineBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationInfo"/> class.
        /// </summary>
        /// <param name="service">The tracked service.</param>
        public ServiceRegistrationInfo(Service service) => _service = service;

        /// <summary>
        /// Gets a value indicating whether the first time a service is requested, initialization (e.g. reading from sources)
        /// happens. This value will then be set to true. Calling many methods on this type before
        /// initialization is an error.
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            private set => _isInitialized = value;
        }

        /// <summary>
        /// Gets or sets a value representing the current initialization depth. Will always be zero for initialized service blocks.
        /// </summary>
        public int InitializationDepth { get; set; }

        /// <summary>
        /// Gets the known implementations. The first implementation is a default one.
        /// </summary>
        public IEnumerable<IComponentRegistration> Implementations
        {
            get
            {
                RequiresInitialization();

                if (_fixedRegistration is object)
                {
                    yield return _fixedRegistration;
                }

                var defaultImpls = _defaultImplementations;

                for (var defaultReverseIdx = defaultImpls.Count - 1; defaultReverseIdx >= 0; defaultReverseIdx--)
                {
                    yield return defaultImpls[defaultReverseIdx];
                }

                if (_sourceImplementations is object)
                {
                    foreach (var item in _sourceImplementations)
                    {
                        yield return item;
                    }
                }

                if (_preserveDefaultImplementations is object)
                {
                    foreach (var item in _preserveDefaultImplementations)
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the service pipeline. Will throw if not initialized.
        /// </summary>
        public IResolvePipeline ServicePipeline => _resolvePipeline ?? throw new InvalidOperationException(ServiceRegistrationInfoResources.NotInitialized);

        /// <summary>
        /// Gets the set of all middleware registered against the service (excluding the default middleware).
        /// </summary>
        public IEnumerable<IResolveMiddleware> ServiceMiddleware
        {
            get
            {
                if (_customPipelineBuilder is null)
                {
                    return Enumerable.Empty<IResolveMiddleware>();
                }

                return _customPipelineBuilder.Middleware.Where(t => !ServicePipelines.IsDefaultMiddleware(t));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RequiresInitialization()
        {
            // Implementations can be read by consumers while we are inside an initialisation window,
            // even when the initialisation hasn't finished yet.
            // The InitialisationDepth property is always 0 outside of the lock-protected initialisation block.
            if (InitializationDepth == 0 && !IsInitialized)
            {
                throw new InvalidOperationException(ServiceRegistrationInfoResources.NotInitialized);
            }
        }

        /// <summary>
        /// Gets a value indicating whether any implementations are known.
        /// </summary>
        public bool IsRegistered
        {
            get
            {
                RequiresInitialization();
                return Any;
            }
        }

        private bool Any =>
            _defaultImplementations.Count > 0 ||
            _sourceImplementations != null ||
            _preserveDefaultImplementations != null;

        /// <summary>
        /// Add an implementation for the service.
        /// </summary>
        /// <param name="registration">The component registration.</param>
        /// <param name="preserveDefaults">Whether to preserve the defaults.</param>
        /// <param name="originatedFromSource">Whether the registration originated from a dynamic source.</param>
        public void AddImplementation(IComponentRegistration registration, bool preserveDefaults, bool originatedFromSource)
        {
            if (registration.Options.HasOption(RegistrationOptions.Fixed))
            {
                if (_fixedRegistration is null || !originatedFromSource)
                {
                    _fixedRegistration = registration;
                }
            }
            else if (preserveDefaults)
            {
                if (originatedFromSource)
                {
                    if (_sourceImplementations == null)
                    {
                        _sourceImplementations = new List<IComponentRegistration>();
                    }

                    _sourceImplementations.Add(registration);
                }
                else
                {
                    if (_preserveDefaultImplementations == null)
                    {
                        _preserveDefaultImplementations = new List<IComponentRegistration>();
                    }

                    _preserveDefaultImplementations.Add(registration);
                }
            }
            else
            {
                if (originatedFromSource)
                {
                    throw new ArgumentOutOfRangeException(nameof(originatedFromSource));
                }

                _defaultImplementations.Add(registration);
            }

            _defaultImplementation = null;
        }

        /// <summary>
        /// Use the specified piece of middleware in the service pipeline.
        /// </summary>
        /// <param name="middleware">The middleware.</param>
        /// <param name="insertionMode">The inserton mode for the pipeline.</param>
        public void UseServiceMiddleware(IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
        {
            if (_customPipelineBuilder is null)
            {
                _customPipelineBuilder = new ResolvePipelineBuilder(PipelineType.Service);
            }

            _customPipelineBuilder.Use(middleware, insertionMode);
        }

        /// <summary>
        /// Use the multiple specified pieces of middleware in the service pipeline.
        /// </summary>
        /// <param name="middleware">The set of middleware.</param>
        /// <param name="insertionMode">The insertion mode.</param>
        public void UseServiceMiddlewareRange(IEnumerable<IResolveMiddleware> middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
        {
            if (!middleware.Any())
            {
                return;
            }

            if (_customPipelineBuilder is null)
            {
                _customPipelineBuilder = new ResolvePipelineBuilder(PipelineType.Service);
            }

            _customPipelineBuilder.UseRange(middleware, insertionMode);
        }

        /// <summary>
        /// Attempts to access the implementing registration for this service, selecting the correct one based on defaults and all known registrations.
        /// </summary>
        /// <param name="registration">The output registration.</param>
        /// <returns>True if a registration was found; false otherwise.</returns>
        public bool TryGetRegistration([NotNullWhen(returnValue: true)] out IComponentRegistration? registration)
        {
            RequiresInitialization();

            registration = _defaultImplementation ??= _fixedRegistration ??
                                                      _defaultImplementations.LastOrDefault() ??
                                                      _sourceImplementations?.First() ??
                                                      _preserveDefaultImplementations?.First();

            return registration != null;
        }

        /// <summary>
        /// Gets a value indicating whether this service info is initialising.
        /// </summary>
        public bool IsInitializing => !IsInitialized && _sourcesToQuery != null;

        /// <summary>
        /// Gets a value indicating whether there are any sources left to query.
        /// </summary>
        public bool HasSourcesToQuery => IsInitializing && _sourcesToQuery!.Count != 0;

        /// <summary>
        /// Begin the initialisation process for this service info, given the set of dynamic sources.
        /// </summary>
        /// <param name="sources">The set of sources.</param>
        public void BeginInitialization(IEnumerable<IRegistrationSource> sources)
        {
            IsInitialized = false;
            _sourcesToQuery = new Queue<IRegistrationSource>(sources);

            // Build the pipeline during service info initialisation, so that sources can access it
            // while getting a registration recursively.
            if (_resolvePipeline is null)
            {
                _resolvePipeline = BuildPipeline();
            }
        }

        /// <summary>
        /// Skip a given source in the set of dynamic sources.
        /// </summary>
        /// <param name="source">The source to skip.</param>
        public void SkipSource(IRegistrationSource source)
        {
            EnforceDuringInitialization();

            _sourcesToQuery = new Queue<IRegistrationSource>(_sourcesToQuery.Where(rs => rs != source));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnforceDuringInitialization()
        {
            if (!IsInitializing)
            {
                throw new InvalidOperationException(ServiceRegistrationInfoResources.NotDuringInitialization);
            }
        }

        /// <summary>
        /// Dequeue the next registration source.
        /// </summary>
        /// <returns>The source.</returns>
        public IRegistrationSource DequeueNextSource()
        {
            EnforceDuringInitialization();

            // _sourcesToQuery always non-null during initialization
            return _sourcesToQuery!.Dequeue();
        }

        /// <summary>
        /// Complete initialisation of the service info.
        /// </summary>
        public void CompleteInitialization()
        {
            EnforceDuringInitialization();

            IsInitialized = true;
            _sourcesToQuery = null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _service.ToString();
        }

        private IResolvePipeline BuildPipeline()
        {
            // Build the custom service pipeline (if we need to).
            if (_customPipelineBuilder is object)
            {
                // Add the default stages.
                _customPipelineBuilder.UseRange(ServicePipelines.DefaultMiddleware);

                // Add the default.
                return _customPipelineBuilder.Build();
            }
            else
            {
                // Nothing custom, use an empty pipeline.
                return ServicePipelines.DefaultServicePipeline;
            }
        }

        /// <summary>
        /// Creates a copy of an uninitialized <see cref="ServiceRegistrationInfo"/>, preserving existing registrations and custom middleware.
        /// </summary>
        /// <returns>A new service registration info block.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the service registration has been initialized already.</exception>
        public ServiceRegistrationInfo CloneUninitialized()
        {
            if (InitializationDepth != 0 || IsInitializing || IsInitialized)
            {
                throw new InvalidOperationException(ServiceRegistrationInfoResources.NotAfterInitialization);
            }

            var copy = new ServiceRegistrationInfo(_service)
            {
                _fixedRegistration = _fixedRegistration,
                _defaultImplementation = _defaultImplementation,
            };

            if (_sourceImplementations is object)
            {
                copy._sourceImplementations = new List<IComponentRegistration>(_sourceImplementations);
            }

            if (_preserveDefaultImplementations is object)
            {
                copy._preserveDefaultImplementations = new List<IComponentRegistration>(_preserveDefaultImplementations);
            }

            copy._defaultImplementations.AddRange(_defaultImplementations);

            if (_customPipelineBuilder is object)
            {
                copy._customPipelineBuilder = _customPipelineBuilder.Clone();
            }

            return copy;
        }

        /// <inheritdoc/>
        IEnumerable<IResolveMiddleware> IResolvePipelineBuilder.Middleware => ServiceMiddleware;

        /// <inheritdoc/>
        PipelineType IResolvePipelineBuilder.Type => PipelineType.Service;

        /// <inheritdoc/>
        IResolvePipeline IResolvePipelineBuilder.Build()
        {
            throw new InvalidOperationException(ServiceRegistrationInfoResources.ServicePipelineCannotBeBuilt);
        }

        /// <inheritdoc/>
        IResolvePipelineBuilder IResolvePipelineBuilder.Use(IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode)
        {
            UseServiceMiddleware(middleware, insertionMode);
            return this;
        }

        /// <inheritdoc/>
        IResolvePipelineBuilder IResolvePipelineBuilder.UseRange(IEnumerable<IResolveMiddleware> middleware, MiddlewareInsertionMode insertionMode)
        {
            UseServiceMiddlewareRange(middleware, insertionMode);
            return this;
        }

        /// <inheritdoc/>
        IResolvePipelineBuilder IResolvePipelineBuilder.Clone()
        {
            if (_customPipelineBuilder is null)
            {
                return new ResolvePipelineBuilder(PipelineType.Service);
            }

            return _customPipelineBuilder.Clone();
        }
    }
}
