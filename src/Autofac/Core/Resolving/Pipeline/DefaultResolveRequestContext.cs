// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Autofac.Features.Decorators;

namespace Autofac.Core.Resolving.Pipeline;

/// <summary>
///     Context area for a resolve request.
/// </summary>
internal sealed class DefaultResolveRequestContext : ResolveRequestContext
{
    private object? _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultResolveRequestContext" /> class.
    /// </summary>
    /// <param name="owningOperation">The owning resolve operation.</param>
    /// <param name="request">The initiating resolve request.</param>
    /// <param name="scope">The lifetime scope.</param>
    /// <param name="diagnosticSource">
    /// The <see cref="DiagnosticListener" /> to which trace events should be written.
    /// </param>
    internal DefaultResolveRequestContext(
        IResolveOperation owningOperation,
        in ResolveRequest request,
        ISharingLifetimeScope scope,
        DiagnosticListener diagnosticSource)
    {
        Operation = owningOperation;
        ActivationScope = scope;
        Parameters = KeyedServiceParameterInjector.AddKeyedServiceParameter(request.Service, request.Parameters, request.Registration);

        // Store only the parts of the request the context exposes, rather than copying the whole struct.
        Registration = request.Registration;
        Service = request.Service;
        DecoratorTarget = request.DecoratorTarget;
        PhaseReached = PipelinePhase.ResolveRequestStart;
        DiagnosticSource = diagnosticSource;
    }

    /// <inheritdoc />
    public override event EventHandler<ResolveRequestCompletingEventArgs>? RequestCompleting;

    /// <inheritdoc />
    public override IResolveOperation Operation
    {
        get;
    }

    /// <inheritdoc />
    public override ISharingLifetimeScope ActivationScope
    {
        get; protected set;
    }

    /// <inheritdoc />
    public override IComponentRegistration Registration
    {
        get;
    }

    /// <inheritdoc />
    public override Service Service
    {
        get;
    }

    /// <inheritdoc />
    public override IComponentRegistration? DecoratorTarget
    {
        get;
    }

    /// <inheritdoc />
    [DisallowNull]
    public override object? Instance
    {
        get => _instance;
        set => _instance = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc />
    public override bool NewInstanceActivated => Instance is not null && PhaseReached == PipelinePhase.Activation;

    /// <inheritdoc />
    public override DiagnosticListener DiagnosticSource
    {
        get;
    }

    /// <inheritdoc />
    public override IEnumerable<Parameter> Parameters
    {
        get; protected set;
    }

    /// <inheritdoc />
    public override PipelinePhase PhaseReached
    {
        get; set;
    }

    /// <inheritdoc />
    public override IComponentRegistry ComponentRegistry => ActivationScope.ComponentRegistry;

    /// <inheritdoc />
    public override DecoratorContext? DecoratorContext
    {
        get; set;
    }

    /// <inheritdoc />
    public override void ChangeScope(ISharingLifetimeScope newScope)
        => ActivationScope = newScope ?? throw new ArgumentNullException(nameof(newScope));

    /// <inheritdoc />
    public override void ChangeParameters(IEnumerable<Parameter> newParameters)
        => Parameters = KeyedServiceParameterInjector.AddKeyedServiceParameter(Service, newParameters ?? throw new ArgumentNullException(nameof(newParameters)), Registration);

    /// <inheritdoc />
    public override object ResolveComponent(in ResolveRequest request)
        => Operation.GetOrCreateInstance(ActivationScope, request);

    /// <summary>
    /// Complete the request, raising any appropriate events.
    /// </summary>
    public void CompleteRequest()
    {
        var handler = RequestCompleting;
        handler?.Invoke(this, new ResolveRequestCompletingEventArgs(this));
    }
}
