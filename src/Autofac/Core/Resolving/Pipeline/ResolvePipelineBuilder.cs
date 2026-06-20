// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Globalization;
using Autofac.Core.Pipeline;
using Autofac.Diagnostics;

namespace Autofac.Core.Resolving.Pipeline;

/// <summary>
/// Provides the functionality to construct a resolve pipeline.
/// </summary>
/// <remarks>
/// <para>
/// The pipeline builder is built as a doubly-linked list; each node in that list is a
/// <see cref="MiddlewareDeclaration"/>, that holds the middleware instance, and the reference to the next and previous nodes.
/// </para>
///
/// <para>
/// When you call one of the Use* methods, we find the appropriate node in the linked list based on the phase of the new middleware
/// and insert it into the list.
/// </para>
///
/// <para>
/// When you build a pipeline, we walk back through that set of middleware and generate the concrete call chain so that when you execute the pipeline,
/// we don't iterate over any nodes, but just invoke the built set of methods.
/// </para>
/// </remarks>
internal class ResolvePipelineBuilder : IResolvePipelineBuilder, IEnumerable<IResolveMiddleware>
{
    /// <summary>
    /// Termination action for the end of pipelines.
    /// </summary>
    private static readonly Action<ResolveRequestContext> _terminateAction = context => { };

    private MiddlewareDeclaration? _first;
    private MiddlewareDeclaration? _last;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolvePipelineBuilder"/> class.
    /// </summary>
    /// <param name="pipelineType">The pipeline type.</param>
    public ResolvePipelineBuilder(PipelineType pipelineType)
    {
        Type = pipelineType;
    }

    /// <inheritdoc/>
    public IEnumerable<IResolveMiddleware> Middleware => this;

    /// <inheritdoc/>
    public PipelineType Type
    {
        get;
    }

    /// <inheritdoc/>
    public IResolvePipelineBuilder Use(IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
    {
        if (middleware is null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        AddStage(middleware, insertionMode);

        return this;
    }

    /// <inheritdoc/>
    public IResolvePipelineBuilder UseRange(IEnumerable<IResolveMiddleware> middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
    {
        if (middleware is null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        // Use multiple stages.
        // Start at the beginning.
        var currentStage = _first;
        using var enumerator = middleware.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return this;
        }

        var lastPhase = enumerator.Current.Phase;
        VerifyPhase(lastPhase);

        if (InsertRangeWithinExistingStages(enumerator, insertionMode, ref currentStage, ref lastPhase))
        {
            return this;
        }

        AppendRemainingStages(enumerator, ref lastPhase);

        return this;
    }

    /// <inheritdoc />
    public IResolvePipeline Build()
    {
        return BuildPipeline(_last);
    }

    /// <inheritdoc/>
    public IResolvePipelineBuilder Clone()
    {
        // To clone a pipeline, we create a new instance, then insert the same stage
        // objects in the same order.
        var newPipeline = new ResolvePipelineBuilder(Type);
        var currentStage = _first;

        while (currentStage is not null)
        {
            newPipeline.AppendStage(currentStage.Middleware);
            currentStage = currentStage.Next;
        }

        return newPipeline;
    }

    /// <inheritdoc/>
    public IEnumerator<IResolveMiddleware> GetEnumerator()
    {
        return new PipelineBuilderEnumerator(_first);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private static ResolvePipeline BuildPipeline(MiddlewareDeclaration? lastDecl)
    {
        // When we build, we go through the set and construct a single call stack, starting from the end.
        var current = lastDecl;
        var currentInvoke = _terminateAction;

        Action<ResolveRequestContext> BuildMiddlewareChain(Action<ResolveRequestContext> next, IResolveMiddleware stage)
        {
            // MetricsEnabled is static readonly (set once at startup), so checking here
            // at pipeline build time avoids a per-invocation branch in every middleware.
            return AutofacMetrics.MetricsEnabled
                ? BuildMetricsMiddlewareChain(next, stage)
                : BuildStandardMiddlewareChain(next, stage);
        }

        Action<ResolveRequestContext> BuildMetricsMiddlewareChain(Action<ResolveRequestContext> next, IResolveMiddleware stage)
        {
            var stagePhase = stage.Phase;
            var stageName = stage.ToString()!;

            // Metrics are captured around each stage execution while preserving
            // diagnostics callbacks (if enabled for the current request).
            return context => ExecuteWithDiagnostics(context, stage, () =>
            {
                context.PhaseReached = stagePhase;
                var timer = ValueStopwatch.StartNew();
                try
                {
                    stage.Execute(context, next);
                }
                finally
                {
                    AutofacMetrics.RecordMiddlewareExecution(stageName, timer.GetElapsedTime());
                }
            });
        }

        Action<ResolveRequestContext> BuildStandardMiddlewareChain(Action<ResolveRequestContext> next, IResolveMiddleware stage)
        {
            var stagePhase = stage.Phase;

            // Hot path when execution metrics are disabled.
            return context => ExecuteWithDiagnostics(context, stage, () =>
            {
                context.PhaseReached = stagePhase;
                stage.Execute(context, next);
            });
        }

        static void ExecuteWithDiagnostics(ResolveRequestContext context, IResolveMiddleware stage, Action action)
        {
            // Same basic flow in if/else, but doing a one-time check for diagnostics
            // and choosing the "diagnostics enabled" version vs. the more common
            // "no diagnostics enabled" path: hot-path optimization.
            if (!context.DiagnosticSource.IsEnabled())
            {
                action();
                return;
            }

            context.DiagnosticSource.MiddlewareStart(context, stage);
            var succeeded = false;
            try
            {
                action();
                succeeded = true;
            }
            finally
            {
                if (succeeded)
                {
                    context.DiagnosticSource.MiddlewareSuccess(context, stage);
                }
                else
                {
                    context.DiagnosticSource.MiddlewareFailure(context, stage);
                }
            }
        }

        while (current is not null)
        {
            var stage = current.Middleware;
            currentInvoke = BuildMiddlewareChain(currentInvoke, stage);
            current = current.Previous;
        }

        return new ResolvePipeline(currentInvoke);
    }

    private bool InsertRangeWithinExistingStages(
        IEnumerator<IResolveMiddleware> enumerator,
        MiddlewareInsertionMode insertionMode,
        ref MiddlewareDeclaration? currentStage,
        ref PipelinePhase lastPhase)
    {
        while (currentStage is not null)
        {
            var shouldInsertBeforeCurrent = insertionMode == MiddlewareInsertionMode.StartOfPhase
                ? currentStage.Middleware.Phase >= enumerator.Current.Phase
                : currentStage.Middleware.Phase > enumerator.Current.Phase;

            if (shouldInsertBeforeCurrent)
            {
                var newDecl = new MiddlewareDeclaration(enumerator.Current);
                InsertBefore(currentStage, newDecl);
                currentStage = newDecl;

                if (!MoveToNextStageAndVerify(enumerator, ref lastPhase))
                {
                    return true;
                }
            }

            currentStage = currentStage.Next;
        }

        return false;
    }

    private void AppendRemainingStages(IEnumerator<IResolveMiddleware> enumerator, ref PipelinePhase lastPhase)
    {
        do
        {
            var nextNewStage = enumerator.Current;

            VerifyPhase(nextNewStage.Phase);

            if (nextNewStage.Phase < lastPhase)
            {
                throw new InvalidOperationException(ResolvePipelineBuilderMessages.MiddlewareMustBeInPhaseOrder);
            }

            lastPhase = nextNewStage.Phase;

            var newStageDecl = new MiddlewareDeclaration(nextNewStage);
            AppendDeclaration(newStageDecl);
        }
        while (enumerator.MoveNext());
    }

    private bool MoveToNextStageAndVerify(IEnumerator<IResolveMiddleware> enumerator, ref PipelinePhase lastPhase)
    {
        if (!enumerator.MoveNext())
        {
            return false;
        }

        var nextPhase = enumerator.Current.Phase;
        VerifyPhase(nextPhase);

        if (nextPhase < lastPhase)
        {
            throw new InvalidOperationException(ResolvePipelineBuilderMessages.MiddlewareMustBeInPhaseOrder);
        }

        lastPhase = nextPhase;
        return true;
    }

    private void InsertBefore(MiddlewareDeclaration currentStage, MiddlewareDeclaration newDecl)
    {
        if (currentStage.Previous is not null)
        {
            // Insert the node.
            currentStage.Previous.Next = newDecl;
            newDecl.Next = currentStage;
            newDecl.Previous = currentStage.Previous;
            currentStage.Previous = newDecl;
            return;
        }

        _first!.Previous = newDecl;
        newDecl.Next = _first;
        _first = newDecl;
    }

    private void AppendDeclaration(MiddlewareDeclaration newStageDecl)
    {
        if (_last is null)
        {
            _first = _last = newStageDecl;
            return;
        }

        newStageDecl.Previous = _last;
        _last.Next = newStageDecl;
        _last = newStageDecl;
    }

    private void AddStage(IResolveMiddleware stage, MiddlewareInsertionMode insertionLocation)
    {
        VerifyPhase(stage.Phase);

        // Start at the beginning.
        var currentStage = _first;

        var newStageDecl = new MiddlewareDeclaration(stage);

        if (_first is null)
        {
            _first = _last = newStageDecl;
            return;
        }

        while (currentStage is not null)
        {
            if (insertionLocation == MiddlewareInsertionMode.StartOfPhase ? currentStage.Middleware.Phase >= stage.Phase : currentStage.Middleware.Phase > stage.Phase)
            {
                if (currentStage.Previous is not null)
                {
                    // Insert the node.
                    currentStage.Previous.Next = newStageDecl;
                    newStageDecl.Next = currentStage;
                    newStageDecl.Previous = currentStage.Previous;
                    currentStage.Previous = newStageDecl;
                }
                else
                {
                    _first.Previous = newStageDecl;
                    newStageDecl.Next = _first;
                    _first = newStageDecl;
                }

                return;
            }

            currentStage = currentStage.Next;
        }

        // Add at the end.
        newStageDecl.Previous = _last;
        _last!.Next = newStageDecl;
        _last = newStageDecl;
    }

    private void AppendStage(IResolveMiddleware stage)
    {
        var newDecl = new MiddlewareDeclaration(stage);

        if (_last is null)
        {
            _first = _last = newDecl;
        }
        else
        {
            newDecl.Previous = _last;
            _last.Next = newDecl;
            _last = newDecl;
        }
    }

    private void VerifyPhase(PipelinePhase middlewarePhase)
    {
        static string DescribeValidEnumRange(PipelinePhase start, PipelinePhase end)
        {
#if NET5_0_OR_GREATER
            var enumValues = Enum.GetValues<PipelinePhase>()
                                 .Where(value => value >= start && value <= end);
#else
            var enumValues = Enum.GetValues(typeof(PipelinePhase))
                                 .Cast<PipelinePhase>()
                                 .Where(value => value >= start && value <= end);
#endif

            return string.Join(", ", enumValues);
        }

        if (Type == PipelineType.Service)
        {
            if (middlewarePhase > PipelinePhase.ServicePipelineEnd)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        ResolvePipelineBuilderMessages.CannotAddRegistrationMiddlewareToServicePipeline,
                        middlewarePhase,
                        DescribeValidEnumRange(PipelinePhase.ResolveRequestStart, PipelinePhase.ServicePipelineEnd)));
            }
        }
        else if (middlewarePhase < PipelinePhase.RegistrationPipelineStart)
        {
            throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        ResolvePipelineBuilderMessages.CannotAddServiceMiddlewareToRegistrationPipeline,
                        middlewarePhase,
                        DescribeValidEnumRange(PipelinePhase.RegistrationPipelineStart, PipelinePhase.Activation)));
        }
    }
}
