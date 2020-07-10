using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Autofac.Core.Resolving;

namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Provides a resolve pipeline tracer that generates DOT graph output
    /// traces for an end-to-end operation flow. Attach to the
    /// <see cref="FullOperationDiagnosticTracerBase.OperationCompleted"/>
    /// event to receive notifications when a new graph is available.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tracer subscribes to all Autofac diagnostic events and can't be
    /// unsubscribed. This is required to ensure beginning and end of each
    /// logical activity can be captured.
    /// </para>
    /// </remarks>
    public class DotDiagnosticTracer : FullOperationDiagnosticTracerBase
    {
        private const string RequestExceptionTraced = "__RequestException";

        private readonly ConcurrentDictionary<ITracingIdentifer, DotGraphBuilder> _operationBuilders = new ConcurrentDictionary<ITracingIdentifer, DotGraphBuilder>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DotDiagnosticTracer"/> class.
        /// </summary>
        public DotDiagnosticTracer()
            : base()
        {
        }

        /// <summary>
        /// Gets the number of operations in progress being traced.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> with the number of trace IDs associated
        /// with in-progress operations being traced by this tracer.
        /// </value>
        public override int OperationsInProgress => _operationBuilders.Count;

        /// <inheritdoc/>
        public override void OnOperationStart(OperationStartDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            var builder = _operationBuilders.GetOrAdd(data.Operation.TracingId, k => new DotGraphBuilder());
        }

        /// <inheritdoc/>
        public override void OnRequestStart(RequestDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                builder.OnRequestStart(
                    data.RequestContext.Service.ToString(),
                    data.RequestContext.Registration.Activator.DisplayName(),
                    data.RequestContext.DecoratorTarget?.Activator.DisplayName());
            }
        }

        /// <inheritdoc/>
        public override void OnMiddlewareStart(MiddlewareDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.RequestContext.Operation.TracingId, out var builder))
            {
                builder.OnMiddlewareStart(data.Middleware.ToString());
            }
        }

        /// <inheritdoc/>
        public override void OnMiddlewareFailure(MiddlewareDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.RequestContext.Operation.TracingId, out var builder))
            {
                builder.OnMiddlewareFailure();
            }
        }

        /// <inheritdoc/>
        public override void OnMiddlewareSuccess(MiddlewareDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.RequestContext.Operation.TracingId, out var builder))
            {
                builder.OnMiddlewareSuccess();
            }
        }

        /// <inheritdoc/>
        public override void OnRequestFailure(RequestFailureDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                var requestException = data.RequestException;
                if (requestException is DependencyResolutionException && requestException.InnerException is object)
                {
                    requestException = requestException.InnerException;
                }

                if (requestException.Data.Contains(RequestExceptionTraced))
                {
                    builder.OnRequestFailure(null);
                }
                else
                {
                    builder.OnRequestFailure(requestException);
                }

                requestException.Data[RequestExceptionTraced] = true;
            }
        }

        /// <inheritdoc/>
        public override void OnRequestSuccess(RequestDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                builder.OnRequestSuccess(data.RequestContext.Instance?.GetType().ToString());
            }
        }

        /// <inheritdoc/>
        public override void OnOperationFailure(OperationFailureDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                try
                {
                    builder.OnOperationFailure(data.OperationException);

                    // If we're completing the root operation, raise the event.
                    if (data.Operation.IsTopLevelOperation)
                    {
                        OnOperationCompleted(new OperationTraceCompletedArgs(data.Operation, builder.ToString()));
                    }
                }
                finally
                {
                    _operationBuilders.TryRemove(data.Operation.TracingId, out var _);
                }
            }
        }

        /// <inheritdoc/>
        public override void OnOperationSuccess(OperationSuccessDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                try
                {
                    builder.OnOperationSuccess(data.ResolvedInstance?.GetType().ToString());

                    // If we're completing the root operation, raise the event.
                    if (data.Operation.IsTopLevelOperation)
                    {
                        OnOperationCompleted(new OperationTraceCompletedArgs(data.Operation, builder.ToString()));
                    }
                }
                finally
                {
                    _operationBuilders.TryRemove(data.Operation.TracingId, out var _);
                }
            }
        }

        private abstract class DotGraphNode
        {
            private const int IndentSize = 2;

            public string Id { get; } = "n" + Guid.NewGuid().ToString("N");

            public List<DotGraphNode> Children { get; } = new List<DotGraphNode>();

            public bool Success { get; set; }

            protected static void AppendIndent(int indent, StringBuilder stringBuilder)
            {
                stringBuilder.Append(' ', indent * IndentSize);
            }

            public virtual void ToString(int indent, StringBuilder stringBuilder)
            {
                foreach (var child in Children)
                {
                    child.ToString(indent + 1, stringBuilder);
                }
            }
        }

        private class ResolveRequestNode : DotGraphNode
        {
            public ResolveRequestNode(string service, string component)
            {
                Service = service;
                Component = component;
            }

            public string Service { get; private set; }

            public string Component { get; private set; }

            public string? DecoratorTarget { get; set; }

            public Exception? Exception { get; set; }

            public string? InstanceType { get; set; }

            public override void ToString(int indent, StringBuilder stringBuilder)
            {
                AppendIndent(indent, stringBuilder);
                stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} [label=\"Resolve Request\"]", Id);
                stringBuilder.AppendLine();
                foreach (var child in Children)
                {
                    AppendIndent(indent, stringBuilder);
                    stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} -> {1}", Id, child.Id);
                    stringBuilder.AppendLine();
                }

                base.ToString(indent, stringBuilder);
            }
        }

        private class OperationNode : DotGraphNode
        {
            public Exception? Exception { get; set; }

            public string? InstanceType { get; set; }

            public override void ToString(int indent, StringBuilder stringBuilder)
            {
                AppendIndent(indent, stringBuilder);
                stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} [label=\"Operation\"]", Id);
                stringBuilder.AppendLine();
                foreach (var child in Children)
                {
                    AppendIndent(indent, stringBuilder);
                    stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} -> {1}", Id, child.Id);
                    stringBuilder.AppendLine();
                }

                base.ToString(indent, stringBuilder);
            }
        }

        private class MiddlewareNode : DotGraphNode
        {
            public MiddlewareNode(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public override void ToString(int indent, StringBuilder stringBuilder)
            {
                AppendIndent(indent, stringBuilder);
                stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} [label=\"Middleware\"]", Id);
                stringBuilder.AppendLine();
                foreach (var child in Children)
                {
                    AppendIndent(indent, stringBuilder);
                    stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "{0} -> {1}", Id, child.Id);
                    stringBuilder.AppendLine();
                }

                base.ToString(indent, stringBuilder);
            }
        }

        /// <summary>
        /// Generator for DOT format graph traces.
        /// </summary>
        private class DotGraphBuilder
        {
            // https://www.graphviz.org/pdf/dotguide.pdf
            // TODO: Wrong tree structure - middleware calls middleware, not request calls all middleware.
            // TODO: Try to render the middleware as a stack that can spawn other requests.
            // TODO: Bold on failure paths.
            // TODO: Actually put real info into the graph (errors, names)
            // TODO: Different shapes per thing - operation, request, MW.
            public OperationNode Root { get; private set; }

            public Stack<OperationNode> Operations { get; } = new Stack<OperationNode>();

            public Stack<ResolveRequestNode> ResolveRequests { get; } = new Stack<ResolveRequestNode>();

            public Stack<MiddlewareNode> Middlewares { get; } = new Stack<MiddlewareNode>();

            public DotGraphBuilder()
            {
                Root = new OperationNode();
                Operations.Push(Root);
            }

            public void OnOperationFailure(Exception? operationException)
            {
                var operation = Operations.Pop();
                operation.Success = false;
                operation.Exception = operationException;
            }

            public void OnOperationSuccess(string? instanceType)
            {
                var operation = Operations.Pop();
                operation.Success = true;
                operation.InstanceType = instanceType;
            }

            public void OnRequestStart(string service, string component, string? decoratorTarget)
            {
                var request = new ResolveRequestNode(service, component);
                if (decoratorTarget is object)
                {
                    request.DecoratorTarget = decoratorTarget;
                }

                Operations.Peek().Children.Add(request);
                ResolveRequests.Push(request);
            }

            public void OnRequestFailure(Exception? requestException)
            {
                var request = ResolveRequests.Pop();
                request.Success = false;
                request.Exception = requestException;
            }

            public void OnRequestSuccess(string? instanceType)
            {
                var request = ResolveRequests.Pop();
                request.Success = true;
                request.InstanceType = instanceType;
            }

            public void OnMiddlewareStart(string middleware)
            {
                var mw = new MiddlewareNode(middleware);
                ResolveRequests.Peek().Children.Add(mw);
                Middlewares.Push(mw);
            }

            public void OnMiddlewareFailure()
            {
                var mw = Middlewares.Pop();
                mw.Success = false;
            }

            public void OnMiddlewareSuccess()
            {
                var mw = Middlewares.Pop();
                mw.Success = true;
            }

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.AppendLine("digraph G {");
                Root.ToString(1, builder);
                builder.AppendLine("}");
                return builder.ToString();
            }
        }
    }
}
