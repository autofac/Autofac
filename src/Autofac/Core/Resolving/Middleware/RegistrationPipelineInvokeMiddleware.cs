// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Diagnostics;

namespace Autofac.Core.Resolving.Middleware;

/// <summary>
/// Middleware added by default to the end of all service pipelines that invokes the registration's pipeline.
/// </summary>
internal class RegistrationPipelineInvokeMiddleware : IResolveMiddleware
{
    private RegistrationPipelineInvokeMiddleware()
    {
    }

    /// <summary>
    /// Gets the singleton instance of this middleware.
    /// </summary>
    public static RegistrationPipelineInvokeMiddleware Instance { get; } = new RegistrationPipelineInvokeMiddleware();

    /// <inheritdoc/>
    public PipelinePhase Phase => PipelinePhase.ServicePipelineEnd;

    /// <inheritdoc/>
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        if (!AutofacMetrics.MetricsEnabled)
        {
            context.Registration.ResolvePipeline.Invoke(context);
            return;
        }

        var start = Stopwatch.GetTimestamp();
        try
        {
            context.Registration.ResolvePipeline.Invoke(context);
        }
        finally
        {
            AutofacMetrics.RecordMiddlewareExecution(nameof(RegistrationPipelineInvokeMiddleware), Stopwatch.GetTimestamp() - start);
        }
    }

    /// <inheritdoc/>
    public override string ToString() => nameof(RegistrationPipelineInvokeMiddleware);
}
