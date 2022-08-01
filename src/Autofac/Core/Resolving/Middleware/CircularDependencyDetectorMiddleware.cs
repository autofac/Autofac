// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using System.Runtime.CompilerServices;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware;

/// <summary>
/// Common stage. Added to the start of all component pipelines.
/// </summary>
internal class CircularDependencyDetectorMiddleware : IResolveMiddleware
{
    /// <summary>
    /// Defines the default max resolve depth.
    /// </summary>
    public const int DefaultMaxResolveDepth = 50;

    private bool _doCircularDependencyChecks = true;

    /// <summary>
    /// Gets the default instance of <see cref="CircularDependencyDetectorMiddleware"/>.
    /// </summary>
    public static CircularDependencyDetectorMiddleware Default { get; } = new CircularDependencyDetectorMiddleware(DefaultMaxResolveDepth);

    private readonly int _maxResolveDepth;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircularDependencyDetectorMiddleware"/> class.
    /// </summary>
    /// <param name="maxResolveDepth">The max resolve depth.</param>
    public CircularDependencyDetectorMiddleware(int maxResolveDepth)
    {
        _maxResolveDepth = maxResolveDepth;
    }

    /// <summary>
    /// Universally disables the circular dependency check on every resolution. Instead,
    /// a circular dependency may be caught after many loops and may cause undesirable
    /// side effects, however improving performance for very deep resolution chains.
    /// </summary>
    public void UnsafeDisableProactiveCircularDependencyChecks()
    {
        _doCircularDependencyChecks = false;
    }

    /// <summary>
    /// Enable the default circular dependency checks.
    /// </summary>
    public void EnableProactiveCircularDependencyChecks()
    {
        _doCircularDependencyChecks = true;
    }

    /// <inheritdoc/>
    public PipelinePhase Phase => PipelinePhase.ResolveRequestStart;

    /// <inheritdoc/>
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        if (context.Operation is not IDependencyTrackingResolveOperation dependencyTrackingResolveOperation)
        {
            // Skipping circular dependency detection, since IResolveOperation is not IDependencyTrackingResolveOperation
            // Which contains the actual RequestStack using SegmentStack
            next(context);
            return;
        }

        var activationDepth = context.Operation.RequestDepth;

        if (activationDepth > _maxResolveDepth)
        {
#if NETSTANDARD2_1
            // In .NET Standard 2.1 we will try and keep going until we run out of stack space.
            if (!RuntimeHelpers.TryEnsureSufficientExecutionStack())
            {
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, CircularDependencyDetectorMessages.MaxDepthExceeded, context.Service));
            }
#else
            // Pre .NET Standard 2.1 we just end at 50.
            throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, CircularDependencyDetectorMessages.MaxDepthExceeded, context.Service));
#endif
        }

        var requestStack = dependencyTrackingResolveOperation.RequestStack;

        // The first one is the current resolve request.
        // Do our circular dependency check.
        if (_doCircularDependencyChecks && activationDepth > 1)
        {
            var registration = context.Registration;

            foreach (var requestEntry in requestStack)
            {
                if (requestEntry.Registration == registration)
                {
                    throw new DependencyResolutionException(string.Format(
                        CultureInfo.CurrentCulture,
                        CircularDependencyDetectorMessages.CircularDependency,
                        CreateDependencyGraphTo(registration, requestStack)));
                }
            }
        }

        requestStack.Push(context);

        try
        {
            // Circular dependency check is done, move to the next stage.
            next(context);
        }
        finally
        {
            requestStack.Pop();
        }
    }

    /// <inheritdoc/>
    public override string ToString() => nameof(CircularDependencyDetectorMiddleware);

    private static string CreateDependencyGraphTo(IComponentRegistration registration, IEnumerable<ResolveRequestContext> requestStack)
    {
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        if (requestStack == null)
        {
            throw new ArgumentNullException(nameof(requestStack));
        }

        var dependencyGraph = Display(registration);

        return requestStack.Select(a => a.Registration)
            .Aggregate(dependencyGraph, (current, requestor) => Display(requestor) + " -> " + current);
    }

    private static string Display(IComponentRegistration registration)
    {
        return registration.Activator.DisplayName();
    }
}
