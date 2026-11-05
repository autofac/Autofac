// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Diagnostics;

namespace Autofac.Core.Resolving;

/// <summary>
/// A <see cref="ResolveOperation"/> is a component context that sequences and monitors the multiple
/// activations that go into producing a single requested object graph.
/// </summary>
internal sealed class ResolveOperation : IDependencyTrackingResolveOperation
{
    private const int SuccessListInitialCapacity = 32;

    // Created on first successful request; operations served from the shared-instance fast path never need it.
    private List<DefaultResolveRequestContext>? _successfulRequests;
    private bool _ended;
    private int _nextCompleteSuccessfulRequestStartPos;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolveOperation" /> class.
    /// </summary>
    /// <param name="mostNestedLifetimeScope"> The most nested scope in which to begin the operation. The operation
    /// can move upward to less nested scopes as components with wider sharing scopes are activated.
    /// </param>
    /// <param name="diagnosticSource">
    /// The <see cref="DiagnosticListener" /> to which trace events should be written.
    /// </param>
    public ResolveOperation(
        ISharingLifetimeScope mostNestedLifetimeScope,
        DiagnosticListener diagnosticSource)
    {
        CurrentScope = mostNestedLifetimeScope ?? throw new ArgumentNullException(nameof(mostNestedLifetimeScope));
        DiagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
    }

    /// <inheritdoc />
    public event EventHandler<ResolveRequestBeginningEventArgs>? ResolveRequestBeginning;

    /// <inheritdoc />
    public event EventHandler<ResolveOperationEndingEventArgs>? CurrentOperationEnding;

    /// <summary>
    /// Gets the active resolve request.
    /// </summary>
    public ResolveRequestContext? ActiveRequestContext
    {
        get; private set;
    }

    /// <summary>
    /// Gets the current lifetime scope of the operation; based on the most recently executed request.
    /// </summary>
    public ISharingLifetimeScope CurrentScope
    {
        get; private set;
    }

    /// <inheritdoc/>
    public IEnumerable<ResolveRequestContext> InProgressRequests => RequestStack;

    /// <summary>
    /// Gets the <see cref="DiagnosticListener" /> for the operation.
    /// </summary>
    public DiagnosticListener DiagnosticSource
    {
        get;
    }

    /// <summary>
    /// Gets the current request depth.
    /// </summary>
    public int RequestDepth
    {
        get; private set;
    }

    /// <inheritdoc/>
    public SegmentedStack<ResolveRequestContext> RequestStack { get; } = new SegmentedStack<ResolveRequestContext>();

    /// <summary>
    /// Gets the <see cref="ResolveRequest" /> that initiated the operation. Other nested requests may have been
    /// issued as a result of this one.
    /// </summary>
    public ResolveRequest? InitiatingRequest
    {
        get; private set;
    }

    /// <summary>
    /// Execute the complete resolve operation.
    /// </summary>
    /// <param name="request">The resolution context.</param>
    /// <returns>The resolved instance.</returns>
    public object Execute(in ResolveRequest request)
    {
        return ExecuteOperation(request);
    }

    /// <summary>
    /// Enter a new dependency chain block where subsequent requests inside the operation are allowed to repeat
    /// registrations from before the block.
    /// </summary>
    /// <returns>A disposable that should be disposed to exit the block.</returns>
    public IDisposable EnterNewDependencyDetectionBlock() => RequestStack.EnterSegment();

    /// <inheritdoc />
    public object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, in ResolveRequest request)
    {
        if (_ended)
        {
            throw new ObjectDisposedException(ResolveOperationResources.TemporaryContextDisposed, innerException: null);
        }

        // Fast-path: if the registration is shared and already cached, skip the entire
        // pipeline (context allocation, middleware chain, etc.). Only safe when
        // no event handlers or diagnostics are observing.
        if (ResolveRequestBeginning is null &&
            !DiagnosticSource.IsEnabled() &&
            TryGetCachedSharedInstance(currentOperationScope, request, out var cached))
        {
            return cached;
        }

        // Create a new request context.
        var requestContext = new DefaultResolveRequestContext(this, request, currentOperationScope, DiagnosticSource);

        // Raise our request-beginning event.
        var handler = ResolveRequestBeginning;
        handler?.Invoke(this, new ResolveRequestBeginningEventArgs(requestContext));

        RequestDepth++;

        // Track the last active request and scope in the call stack.
        var lastActiveRequest = ActiveRequestContext;
        var lastScope = CurrentScope;

        ActiveRequestContext = requestContext;
        CurrentScope = currentOperationScope;

        try
        {
            // Same basic flow in if/else, but doing a one-time check for diagnostics
            // and choosing the "diagnostics enabled" version vs. the more common
            // "no diagnostics enabled" path: hot-path optimization.
            if (DiagnosticSource.IsEnabled())
            {
                DiagnosticSource.RequestStart(this, requestContext);
                InvokePipeline(request, requestContext);
                DiagnosticSource.RequestSuccess(this, requestContext);
            }
            else
            {
                InvokePipeline(request, requestContext);
            }
        }
        catch (Exception ex)
        {
            if (DiagnosticSource.IsEnabled())
            {
                DiagnosticSource.RequestFailure(this, requestContext, ex);
            }

            throw;
        }
        finally
        {
            ActiveRequestContext = lastActiveRequest;
            CurrentScope = lastScope;

            // Raise the appropriate completion events.
            if (RequestStack.Count == 0)
            {
                CompleteRequests();
            }

            RequestDepth--;
        }

        // InvokePipeline throws if the instance is null but
        // analyzers don't pick that up and get mad.
        return requestContext.Instance!;
    }

    /// <summary>
    /// Gets the instance the default service pipeline would return from its sharing stage, if one is already cached.
    /// Callers must check that nothing observes the request (event handlers or diagnostics) before using it.
    /// </summary>
    /// <param name="currentOperationScope">The scope the request is made in.</param>
    /// <param name="request">The resolve request.</param>
    /// <param name="instance">The cached instance.</param>
    /// <returns>True if the request can be answered without running its pipeline.</returns>
    internal static bool TryGetCachedSharedInstance(ISharingLifetimeScope currentOperationScope, in ResolveRequest request, [NotNullWhen(true)] out object? instance)
    {
        var registration = request.Registration;

        // A custom service pipeline (decorators or service middleware) has to run.
        if (registration.Sharing != InstanceSharing.Shared || request.ResolvePipeline != ServicePipelines.DefaultServicePipeline)
        {
            instance = null;
            return false;
        }

        var scope = FindSharingScope(registration.Lifetime, currentOperationScope);
        if (scope is null)
        {
            // No matching scope; the pipeline reports that.
            instance = null;
            return false;
        }

        return scope.TryGetSharedInstance(registration.Id, request.DecoratorTarget?.Id, out instance);
    }

    /// <summary>
    /// Finds the scope the default pipeline's scope selection would pick for the built-in lifetimes, without throwing.
    /// Any other lifetime is looked up in the current scope.
    /// </summary>
    private static ISharingLifetimeScope? FindSharingScope(IComponentLifetime lifetime, ISharingLifetimeScope currentOperationScope)
    {
        // Singletons live in the root scope.
        if (ReferenceEquals(lifetime, RootScopeLifetime.Instance))
        {
            return currentOperationScope.RootLifetimeScope;
        }

        // Exact type: a derived class could re-implement IComponentLifetime.FindScope differently.
        if (lifetime.GetType() == typeof(MatchingScopeLifetime))
        {
            return ((MatchingScopeLifetime)lifetime).TryFindScope(currentOperationScope);
        }

        return currentOperationScope;
    }

    /// <summary>
    /// Invoke this method to execute the operation for a given request.
    /// </summary>
    /// <param name="request">The resolve request.</param>
    /// <returns>The resolved instance.</returns>
    private object ExecuteOperation(in ResolveRequest request)
    {
        object result;

        try
        {
            InitiatingRequest = request;
            if (DiagnosticSource.IsEnabled())
            {
                DiagnosticSource.OperationStart(this, request);
            }

            result = GetOrCreateInstance(CurrentScope, request);
        }
        catch (ObjectDisposedException disposeException)
        {
            if (DiagnosticSource.IsEnabled())
            {
                DiagnosticSource.OperationFailure(this, disposeException);
            }

            throw;
        }
        catch (DependencyResolutionException dependencyResolutionException)
        {
            if (DiagnosticSource.IsEnabled())
            {
                DiagnosticSource.OperationFailure(this, dependencyResolutionException);
            }

            End(dependencyResolutionException);
            throw;
        }
        catch (Exception exception)
        {
            End(exception);
            if (DiagnosticSource.IsEnabled())
            {
                DiagnosticSource.OperationFailure(this, exception);
            }

            throw new DependencyResolutionException(ResolveOperationResources.ExceptionDuringResolve, exception);
        }
        finally
        {
            ResetSuccessfulRequests();
        }

        End();

        if (DiagnosticSource.IsEnabled())
        {
            DiagnosticSource.OperationSuccess(this, result);
        }

        return result;
    }

    /// <summary>
    /// Basic pipeline invocation steps used when retrieving an instance. Isolated
    /// to enable it to be optionally surrounded with diagnostics.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InvokePipeline(in ResolveRequest request, DefaultResolveRequestContext requestContext)
    {
        request.ResolvePipeline.Invoke(requestContext);
        if (requestContext.Instance == null)
        {
            throw new DependencyResolutionException(ResolveOperationResources.PipelineCompletedWithNoInstance);
        }

        (_successfulRequests ??= new(SuccessListInitialCapacity)).Add(requestContext);
    }

    private void CompleteRequests()
    {
        var completed = _successfulRequests;
        if (completed is null)
        {
            return;
        }

        var count = completed.Count;
        var startPosition = _nextCompleteSuccessfulRequestStartPos;
        ResetSuccessfulRequests();

        for (var i = startPosition; i < count; i++)
        {
            completed[i].CompleteRequest();
        }
    }

    private void ResetSuccessfulRequests()
    {
        _nextCompleteSuccessfulRequestStartPos = _successfulRequests?.Count ?? 0;
    }

    private void End(Exception? exception = null)
    {
        if (_ended)
        {
            return;
        }

        _ended = true;
        var handler = CurrentOperationEnding;
        handler?.Invoke(this, new ResolveOperationEndingEventArgs(this, exception));
    }
}
