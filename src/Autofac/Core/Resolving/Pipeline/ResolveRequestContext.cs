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

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Context area for a resolve request.
    /// </summary>
    internal sealed class ResolveRequestContext : IResolveRequestContext
    {
        private readonly ResolveRequest _resolveRequest;
        private object? _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveRequestContext"/> class.
        /// </summary>
        /// <param name="owningOperation">The owning resolve operation.</param>
        /// <param name="request">The initiating resolve request.</param>
        /// <param name="scope">The lifetime scope.</param>
        /// <param name="tracer">An optional tracer.</param>
        internal ResolveRequestContext(
            IPipelineResolveOperation owningOperation,
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

        /// <inheritdoc />
        public IPipelineResolveOperation Operation { get; }

        /// <inheritdoc />
        public IComponentRegistration Registration => _resolveRequest.Registration;

        /// <inheritdoc />
        public Service Service => _resolveRequest.Service;

        /// <inheritdoc />
        public IComponentRegistration? DecoratorTarget => _resolveRequest.DecoratorTarget;

        /// <inheritdoc />
        public ISharingLifetimeScope ActivationScope { get; private set; }

        /// <inheritdoc />
        [DisallowNull]
        public object? Instance
        {
            get => _instance;
            set => _instance = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public IEnumerable<Parameter> Parameters { get; private set; }

        /// <inheritdoc />
        public IComponentRegistry ComponentRegistry => ActivationScope.ComponentRegistry;

        /// <inheritdoc />
        public PipelinePhase PhaseReached { get; set; }

        /// <inheritdoc />
        public IResolvePipelineTracer? Tracer { get; }

        /// <inheritdoc />
        public bool NewInstanceActivated => Instance is object && PhaseReached == PipelinePhase.Activation;

        /// <inheritdoc />
        public IResolvePipeline? Continuation { get; set; }

        /// <inheritdoc />
        public event EventHandler<ResolveRequestCompletingEventArgs>? RequestCompleting;

        /// <inheritdoc />
        public object ResolveComponent(ResolveRequest request)
        {
            return Operation.GetOrCreateInstance(ActivationScope, request);
        }

        /// <inheritdoc />
        public void ChangeParameters(IEnumerable<Parameter> newParameters)
        {
            Parameters = newParameters ?? throw new ArgumentNullException(nameof(newParameters));
        }

        /// <inheritdoc />
        public void ChangeScope(ISharingLifetimeScope newScope)
        {
            ActivationScope = newScope ?? throw new ArgumentNullException(nameof(newScope));
        }

        public void Complete()
        {
            var handler = RequestCompleting;
            handler?.Invoke(this, new ResolveRequestCompletingEventArgs(this));
        }

        /// <inheritdoc />
        void IResolveRequestContext.SetPhase(PipelinePhase phase)
        {
            PhaseReached = phase;
        }

        /// <inheritdoc />
        public object ResolveComponentWithNewOperation(ResolveRequest request)
        {
            // Create a new operation, with the current ActivationScope and Tracer.
            // Pass in the current operation as a tracing reference.
            var operation = new ResolveOperation(ActivationScope, Tracer, Operation);
            return operation.Execute(request);
        }
    }
}
