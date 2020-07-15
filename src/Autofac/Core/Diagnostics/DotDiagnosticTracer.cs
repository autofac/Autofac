// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Autofac.Core.Resolving;

namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Provides a resolve pipeline tracer that generates DOT graph output
    /// traces for an end-to-end operation flow. Attach to the
    /// <see cref="OperationDiagnosticTracerBase.OperationCompleted"/>
    /// event to receive notifications when a new graph is available.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tracer subscribes to all Autofac diagnostic events and can't be
    /// unsubscribed. This is required to ensure beginning and end of each
    /// logical activity can be captured.
    /// </para>
    /// </remarks>
    public class DotDiagnosticTracer : OperationDiagnosticTracerBase
    {
        /// <summary>
        /// Metadata flag to help deduplicate the number of places where the exception is traced.
        /// </summary>
        private const string RequestExceptionTraced = "__RequestException";

        private readonly ConcurrentDictionary<ResolveOperationBase, DotGraphBuilder> _operationBuilders = new ConcurrentDictionary<ResolveOperationBase, DotGraphBuilder>();

        private static readonly string[] DotEvents = new string[]
        {
            DiagnosticEventKeys.OperationStart,
            DiagnosticEventKeys.OperationFailure,
            DiagnosticEventKeys.OperationSuccess,
            DiagnosticEventKeys.RequestStart,
            DiagnosticEventKeys.RequestFailure,
            DiagnosticEventKeys.RequestSuccess,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DotDiagnosticTracer"/> class.
        /// </summary>
        public DotDiagnosticTracer()
            : base(DotEvents)
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

            var builder = _operationBuilders.GetOrAdd(data.Operation, k => new DotGraphBuilder());
            builder.OnOperationStart(data.Operation.InitiatingRequest?.Service.Description);
        }

        /// <inheritdoc/>
        public override void OnOperationSuccess(OperationSuccessDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
            {
                try
                {
                    builder.OnOperationSuccess();
                    OnOperationCompleted(new OperationTraceCompletedArgs(data.Operation, builder.ToString()));
                }
                finally
                {
                    _operationBuilders.TryRemove(data.Operation, out var _);
                }
            }
        }

        /// <inheritdoc/>
        public override void OnOperationFailure(OperationFailureDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
            {
                try
                {
                    builder.OnOperationFailure();
                    OnOperationCompleted(new OperationTraceCompletedArgs(data.Operation, builder.ToString()));
                }
                finally
                {
                    _operationBuilders.TryRemove(data.Operation, out var _);
                }
            }
        }

        /// <inheritdoc/>
        public override void OnRequestStart(RequestDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
            {
                builder.OnRequestStart(
                    data.RequestContext.Service.ToString(),
                    data.RequestContext.Registration.Activator.DisplayName(),
                    data.RequestContext.DecoratorTarget?.Activator.DisplayName());
            }
        }

        /// <inheritdoc/>
        public override void OnRequestSuccess(RequestDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
            {
                builder.OnRequestSuccess(data.RequestContext.Instance?.GetType().ToString());
            }
        }

        /// <inheritdoc/>
        public override void OnRequestFailure(RequestFailureDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
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

        private abstract class DotGraphNode
        {
            public string Id { get; } = "n" + Guid.NewGuid().ToString("N");

            public bool Success { get; set; }

            public List<DotGraphNode> Children { get; } = new List<DotGraphNode>();

            public abstract void ToString(StringBuilder stringBuilder);
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

            public override void ToString(StringBuilder stringBuilder)
            {
                var shape = DecoratorTarget == null ? "component" : "box3d";
                stringBuilder.StartNode(Id, shape, Success)
                    .AppendTableHeader(Service)
                    .AppendTableRow(TracerMessages.ComponentDisplay, Component);

                if (DecoratorTarget is object)
                {
                    stringBuilder.AppendTableRow(TracerMessages.TargetDisplay, DecoratorTarget);
                }

                if (InstanceType is object)
                {
                    stringBuilder.AppendTableRow(TracerMessages.InstanceDisplay, InstanceType);
                }

                if (Exception is object)
                {
                    stringBuilder.AppendTableErrorRow(Exception.GetType().FullName, Exception.Message);
                }

                stringBuilder.EndNode();
                foreach (var child in Children)
                {
                    child.ToString(stringBuilder);
                    stringBuilder.ConnectNodes(Id, child.Id, !child.Success);
                }
            }
        }

        private class OperationNode : DotGraphNode
        {
            public string? Service { get; set; }

            public override void ToString(StringBuilder stringBuilder)
            {
                // Graph header
                stringBuilder.Append("label=<");
                if (!Success)
                {
                    stringBuilder.Append("<b>");
                }

                stringBuilder.Append(HttpUtility.HtmlEncode(Service));
                if (!Success)
                {
                    stringBuilder.Append("</b>");
                }

                stringBuilder.Append(">;");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("labelloc=t");
                foreach (var child in Children)
                {
                    // We _should_ only have one child here - the
                    // initiating request.
                    child.ToString(stringBuilder);
                }
            }
        }

        /// <summary>
        /// Generator for DOT format graph traces.
        /// </summary>
        private class DotGraphBuilder
        {
            public OperationNode Root { get; private set; }

            public Stack<ResolveRequestNode> ResolveRequests { get; } = new Stack<ResolveRequestNode>();

            public DotGraphBuilder()
            {
                Root = new OperationNode();
            }

            public void OnOperationStart(string? service)
            {
                Root.Service = service;
            }

            public void OnOperationFailure()
            {
                Root.Success = false;
            }

            public void OnOperationSuccess()
            {
                Root.Success = true;
            }

            public void OnRequestStart(string service, string component, string? decoratorTarget)
            {
                var request = new ResolveRequestNode(service, component);
                if (decoratorTarget is object)
                {
                    request.DecoratorTarget = decoratorTarget;
                }

                if (ResolveRequests.Count != 0)
                {
                    ResolveRequests.Peek().Children.Add(request);
                }
                else
                {
                    // The initiating request will be the first request
                    // we see, and should be the last one off the stack.
                    Root.Children.Add(request);
                }

                ResolveRequests.Push(request);
            }

            public void OnRequestFailure(Exception? requestException)
            {
                if (ResolveRequests.Count == 0)
                {
                    // OnRequestFailure happened without a corresponding OnRequestStart.
                    return;
                }

                var request = ResolveRequests.Pop();
                request.Success = false;
                request.Exception = requestException;
            }

            public void OnRequestSuccess(string? instanceType)
            {
                if (ResolveRequests.Count == 0)
                {
                    // OnRequestSuccess happened without a corresponding OnRequestStart.
                    return;
                }

                var request = ResolveRequests.Pop();
                request.Success = true;
                request.InstanceType = instanceType;
            }

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.AppendLine("digraph G {");
                Root.ToString(builder);
                builder.AppendLine("}");
                return builder.ToString();
            }
        }
    }
}
