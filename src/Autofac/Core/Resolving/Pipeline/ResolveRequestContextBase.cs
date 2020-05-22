// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core.Diagnostics;
using Autofac.Core.Registration;

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Defines the context object for a single resolve request. Provides access to the in-flight status of the operation,
    /// and ways to manipulate the contents.
    /// </summary>
    public abstract class ResolveRequestContextBase : IComponentContext
    {
        private readonly ResolveRequest _resolveRequest;
        private object? _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveRequestContextBase"/> class.
        /// </summary>
        /// <param name="owningOperation">The owning resolve operation.</param>
        /// <param name="request">The initiating resolve request.</param>
        /// <param name="scope">The lifetime scope.</param>
        /// <param name="tracer">An optional tracer.</param>
        internal ResolveRequestContextBase(
            ResolveOperationBase owningOperation,
            ResolveRequest request,
            ISharingLifetimeScope scope,
            IResolvePipelineTracer? tracer)
        {
            Operation = owningOperation;
            ActivationScope = scope;
            Parameters = request.Parameters;
            PhaseReached = PipelinePhase.RequestStart;
            Tracer = tracer;
            _resolveRequest = request;
        }

        /// <summary>
        /// Gets a reference to the owning resolve operation (which might emcompass multiple nested requests).
        /// </summary>
        public ResolveOperationBase Operation { get; }

        /// <summary>
        /// Gets the lifetime scope that will be used for the activation of any components later in the pipeline.
        /// Avoid resolving instances directly from this scope; they will not be traced as part of the same operation.
        /// </summary>
        public ISharingLifetimeScope ActivationScope { get; private set; }

        /// <summary>
        /// Gets the component registration that is being resolved in the current request.
        /// </summary>
        public IComponentRegistration Registration => _resolveRequest.Registration;

        /// <summary>
        /// Gets the service that is being resolved in the current request.
        /// </summary>
        public Service Service => _resolveRequest.Service;

        /// <summary>
        /// Gets the target registration for decorator requests.
        /// </summary>
        public IComponentRegistration? DecoratorTarget => _resolveRequest.DecoratorTarget;

        /// <summary>
        /// Gets or sets the instance that will be returned as the result of the resolve request.
        /// On the way back up the pipeline, after calling next(ctxt), this value will be populated
        /// with the resolved instance. Check the <see cref="NewInstanceActivated"/> property to determine
        /// whether the object here was a newly activated instance, or a shared instance previously activated.
        /// </summary>
        [DisallowNull]
        public object? Instance
        {
            get => _instance;
            set => _instance = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets a value indicating whether the resolved <see cref="Instance"/> is a new instance of a component has been activated during this request,
        /// or an existing shared instance that has been retrieved.
        /// </summary>
        public bool NewInstanceActivated => Instance is object && PhaseReached == PipelinePhase.Activation;

        /// <summary>
        /// Gets the active <see cref="IResolvePipelineTracer"/> for the request.
        /// </summary>
        public IResolvePipelineTracer? Tracer { get; }

        /// <summary>
        /// Gets the current resolve parameters. These can be changed using the <see cref="ChangeParameters(IEnumerable{Parameter})"/> method.
        /// </summary>
        public IEnumerable<Parameter> Parameters { get; private set; }

        /// <summary>
        /// Gets the phase of the pipeline reached by this request.
        /// </summary>
        public PipelinePhase PhaseReached { get; internal set; }

        /// <summary>
        /// Gets or sets an optional pipeline to invoke at the end of the current request's pipeline.
        /// </summary>
        public IResolvePipeline? Continuation { get; set; }

        /// <inheritdoc />
        public IComponentRegistry ComponentRegistry => ActivationScope.ComponentRegistry;

        /// <summary>
        /// Provides an event that will fire when the current request completes.
        /// Requests will only be considered 'complete' when the overall <see cref="IResolveOperation"/> is completing.
        /// </summary>
        public event EventHandler<ResolveRequestCompletingEventArgs>? RequestCompleting;

        /// <summary>
        /// Use this method to change the <see cref="ISharingLifetimeScope"/> that is used in this request. Changing this scope will
        /// also change the <see cref="IComponentRegistry"/> available in this context.
        /// </summary>
        /// <param name="newScope">The new lifetime scope.</param>
        public void ChangeScope(ISharingLifetimeScope newScope)
        {
            ActivationScope = newScope ?? throw new ArgumentNullException(nameof(newScope));
        }

        /// <summary>
        /// Change the set of parameters being used in the processing of this request.
        /// </summary>
        /// <param name="newParameters">The new set of parameters.</param>
        public void ChangeParameters(IEnumerable<Parameter> newParameters)
        {
            Parameters = newParameters ?? throw new ArgumentNullException(nameof(newParameters));
        }

        /// <inheritdoc />
        public abstract object ResolveComponent(ResolveRequest request);

        /// <summary>
        /// Resolve an instance of the provided registration within the context, but isolated inside a new
        /// <see cref="ResolveOperationBase"/>.
        /// This method should only be used instead of <see cref="IComponentContext.ResolveComponent(ResolveRequest)"/>
        /// if you need to resolve a component with a completely separate operation and circular dependency verification stack.
        /// </summary>
        /// <param name="request">The resolve request.</param>
        /// <returns>
        /// The component instance.
        /// </returns>
        /// <exception cref="ComponentNotRegisteredException"/>
        /// <exception cref="DependencyResolutionException"/>
        public abstract object ResolveComponentWithNewOperation(ResolveRequest request);

        /// <summary>
        /// Complete the request, raising any appropriate events.
        /// </summary>
        protected void CompleteRequest()
        {
            var handler = RequestCompleting;
            handler?.Invoke(this, new ResolveRequestCompletingEventArgs(this));
        }
    }
}
