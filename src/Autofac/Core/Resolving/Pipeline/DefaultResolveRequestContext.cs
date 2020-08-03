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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Autofac.Features.Decorators;

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    ///     Context area for a resolve request.
    /// </summary>
    internal sealed class DefaultResolveRequestContext : ResolveRequestContext
    {
        private readonly ResolveRequest _resolveRequest;
        private object? _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResolveRequestContext" /> class.
        /// </summary>
        /// <param name="owningOperation">The owning resolve operation.</param>
        /// <param name="request">The initiating resolve request.</param>
        /// <param name="scope">The lifetime scope.</param>
        /// <param name="diagnosticSource">
        /// The <see cref="System.Diagnostics.DiagnosticListener" /> to which trace events should be written.
        /// </param>
        internal DefaultResolveRequestContext(
            IResolveOperation owningOperation,
            ResolveRequest request,
            ISharingLifetimeScope scope,
            DiagnosticListener diagnosticSource)
        {
            Operation = owningOperation;
            ActivationScope = scope;
            Parameters = request.Parameters;
            _resolveRequest = request ?? throw new ArgumentNullException(nameof(request));
            PhaseReached = PipelinePhase.ResolveRequestStart;
            DiagnosticSource = diagnosticSource;
        }

        /// <inheritdoc />
        public override IResolveOperation Operation { get; }

        /// <inheritdoc />
        public override ISharingLifetimeScope ActivationScope { get; protected set; }

        /// <inheritdoc />
        public override IComponentRegistration Registration => _resolveRequest.Registration;

        /// <inheritdoc />
        public override Service Service => _resolveRequest.Service;

        /// <inheritdoc />
        public override IComponentRegistration? DecoratorTarget => _resolveRequest.DecoratorTarget;

        /// <inheritdoc />
        [DisallowNull]
        public override object? Instance
        {
            get => _instance;
            set => _instance = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public override bool NewInstanceActivated => Instance is object && PhaseReached == PipelinePhase.Activation;

        /// <inheritdoc />
        public override DiagnosticListener DiagnosticSource { get; }

        /// <inheritdoc />
        public override IEnumerable<Parameter> Parameters { get; protected set; }

        /// <inheritdoc />
        public override PipelinePhase PhaseReached { get; set; }

        /// <inheritdoc />
        public override IComponentRegistry ComponentRegistry => ActivationScope.ComponentRegistry;

        /// <inheritdoc />
        public override event EventHandler<ResolveRequestCompletingEventArgs>? RequestCompleting;

        /// <inheritdoc />
        public override DecoratorContext? DecoratorContext { get; set; }

        /// <inheritdoc />
        public override void ChangeScope(ISharingLifetimeScope newScope) =>
            ActivationScope = newScope ?? throw new ArgumentNullException(nameof(newScope));

        /// <inheritdoc />
        public override void ChangeParameters(IEnumerable<Parameter> newParameters) =>
            Parameters = newParameters ?? throw new ArgumentNullException(nameof(newParameters));

        /// <inheritdoc />
        public override object ResolveComponent(ResolveRequest request) =>
            Operation.GetOrCreateInstance(ActivationScope, request);

        /// <summary>
        /// Complete the request, raising any appropriate events.
        /// </summary>
        public void CompleteRequest()
        {
            EventHandler<ResolveRequestCompletingEventArgs>? handler = RequestCompleting;
            handler?.Invoke(this, new ResolveRequestCompletingEventArgs(this));
        }
    }
}
