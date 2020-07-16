// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
                    data.RequestContext.Service,
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
                builder.OnRequestSuccess(data.RequestContext.Instance);
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

        /// <summary>
        /// One node in the graph. The resulting instance (for successful requests)
        /// is what uniquely identifies the node when normalizing the data. This
        /// converts the notion of a "resolve request" into a dependency graph
        /// based on completed resolutions.
        /// </summary>
        private class ResolveRequestNode
        {
            public ResolveRequestNode(string component)
            {
                Services = new Dictionary<Service, Guid>();
                Component = component;
                Id = Guid.NewGuid();
                Edges = new HashSet<GraphEdge>();
            }

            public Guid Id { get; }

            public Dictionary<Service, Guid> Services { get; private set; }

            public string Component { get; private set; }

            public string? DecoratorTarget { get; set; }

            public bool Success { get; set; }

            public Exception? Exception { get; set; }

            public object? Instance { get; set; }

            public HashSet<GraphEdge> Edges { get; }

            public void ToString(StringBuilder stringBuilder, RequestDictionary allRequests)
            {
                var shape = DecoratorTarget == null ? "component" : "box3d";
                stringBuilder.StartNode(Id, shape, Success);
                foreach (var service in Services.Keys)
                {
                    stringBuilder.AppendServiceRow(service.Description, Services[service]);
                }

                stringBuilder.AppendTableRow(TracerMessages.ComponentDisplay, Component);

                if (DecoratorTarget is object)
                {
                    stringBuilder.AppendTableRow(TracerMessages.TargetDisplay, DecoratorTarget);
                }

                if (Instance is object)
                {
                    stringBuilder.AppendTableRow(TracerMessages.InstanceDisplay, Instance.GetType().FullName);
                }

                if (Exception is object)
                {
                    stringBuilder.AppendTableErrorRow(Exception.GetType().FullName, Exception.Message);
                }

                stringBuilder.EndNode();
                foreach (var edge in Edges)
                {
                    // Connect into a table with the ID format "parent:tablerow"
                    var destination = allRequests[edge.Request];
                    var edgeId = destination.Id.NodeId() + ":" + destination.Services[edge.Service].NodeId();
                    stringBuilder.ConnectNodes(Id.NodeId(), edgeId, edge.Service.Description, !destination.Success);
                }
            }
        }

        /// <summary>
        /// Metadata about the operation being graphed. Used to
        /// generate the graph header.
        /// </summary>
        private class OperationNode
        {
            public string? Service { get; set; }

            public bool Success { get; set; }

            public void ToString(StringBuilder stringBuilder)
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
            }
        }

        /// <summary>
        /// Convenience collection for accessing a request by ID
        /// out of the list of all requests.
        /// </summary>
        private class RequestDictionary : KeyedCollection<Guid, ResolveRequestNode>
        {
            protected override Guid GetKeyForItem(ResolveRequestNode item)
            {
                return item.Id;
            }
        }

        /// <summary>
        /// An edge that connects two nodes (two resolve requests) in a graph.
        /// The source of an edge is the request that's resolving child items;
        /// the target is a specific service on a child request.
        /// </summary>
        private class GraphEdge : IEquatable<GraphEdge>
        {
            public GraphEdge(Guid request, Service service)
            {
                Request = request;
                Service = service ?? throw new ArgumentNullException(nameof(service));
            }

            public Guid Request { get; private set; }

            public Service Service { get; private set; }

            public bool Equals(GraphEdge? other)
            {
                return
                    other is object &&
                    other.Request == Request &&
                    ((other.Service is object && Service is object && other.Service.Equals(Service)) ||
                    (other.Service is null && Service is null));
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as GraphEdge);
            }

            public override int GetHashCode()
            {
                // This doesn't have to be great; we don't really use it
                // but analyzers complain since we do need equality.
                return Request.GetHashCode() ^ Service.GetHashCode();
            }
        }

        /// <summary>
        /// Equality comparer that determines if two resolve requests are effectively the
        /// same based on the returned instance. Used to find "duplicates" in the graph
        /// during normalization.
        /// </summary>
        private class InstanceEqualityComparer : IEqualityComparer<ResolveRequestNode>
        {
            public static InstanceEqualityComparer Default { get; } = new InstanceEqualityComparer();

            public bool Equals(ResolveRequestNode x, ResolveRequestNode y) => ReferenceEquals(x.Instance, y.Instance);

            public int GetHashCode(ResolveRequestNode obj) => RuntimeHelpers.GetHashCode(obj.Instance);
        }

        /// <summary>
        /// Generator for DOT format graph traces.
        /// </summary>
        private class DotGraphBuilder
        {
            /// <summary>
            /// Gets the node that has operation-level data for the graph.
            /// </summary>
            public OperationNode Operation { get; private set; }

            /// <summary>
            /// Gets the set of all requests made during the operation.
            /// </summary>
            public RequestDictionary Requests { get; private set; }

            /// <summary>
            /// Gets the originating request ID. This will also be the first request in the
            /// stack of ongoing requests. Tracked to ensure we retain the originating
            /// request during the normalization of the graph.
            /// </summary>
            public Guid OriginatingRequest { get; private set; }

            /// <summary>
            /// Gets the stack of ongoing requests. The first request in the stack is the originating
            /// request where the graph should start.
            /// </summary>
            public Stack<Guid> CurrentRequest { get; private set; }

            public DotGraphBuilder()
            {
                Operation = new OperationNode();
                Requests = new RequestDictionary();
                CurrentRequest = new Stack<Guid>();
            }

            public void OnOperationStart(string? service)
            {
                Operation.Service = service;
            }

            public void OnOperationFailure()
            {
                Operation.Success = false;
                NormalizeGraph();
            }

            public void OnOperationSuccess()
            {
                Operation.Success = true;
                NormalizeGraph();
            }

            public void OnRequestStart(Service service, string component, string? decoratorTarget)
            {
                var request = new ResolveRequestNode(component);
                request.Services.Add(service, Guid.NewGuid());
                Requests.Add(request);
                if (decoratorTarget is object)
                {
                    request.DecoratorTarget = decoratorTarget;
                }

                if (CurrentRequest.Count != 0)
                {
                    // We're already in a request, so add an edge from
                    // the parent to this new request/service.
                    var parent = Requests[CurrentRequest.Peek()];
                    parent.Edges.Add(new GraphEdge(request.Id, service));
                }
                else
                {
                    // The initiating request will be the first request we see.
                    OriginatingRequest = request.Id;
                }

                // The inbound request is the new current.
                CurrentRequest.Push(request.Id);
            }

            public void OnRequestFailure(Exception? requestException)
            {
                if (CurrentRequest.Count == 0)
                {
                    // OnRequestFailure happened without a corresponding OnRequestStart.
                    return;
                }

                var request = Requests[CurrentRequest.Pop()];
                request.Success = false;
                request.Exception = requestException;
            }

            public void OnRequestSuccess(object? instance)
            {
                if (CurrentRequest.Count == 0)
                {
                    // OnRequestSuccess happened without a corresponding OnRequestStart.
                    return;
                }

                var request = Requests[CurrentRequest.Pop()];
                request.Success = true;
                request.Instance = instance;
            }

            private void NormalizeGraph()
            {
                // Remove any duplicates of the root node. We need to make sure that node in particular stays.
                RemoveDuplicates(OriginatingRequest);

                // Other than the originating request, find the rest of the distinct values
                // so we can de-dupe.
                var unique = Requests
                    .Where(r => r.Success && r.Instance is object && r.Id != OriginatingRequest)
                    .Distinct(InstanceEqualityComparer.Default)
                    .Select(r => r.Id)
                    .ToArray();

                foreach (var id in unique)
                {
                    RemoveDuplicates(id);
                }
            }

            private void RemoveDuplicates(Guid sourceId)
            {
                var source = Requests[sourceId];
                if (!source.Success || source.Instance is null)
                {
                    // We can only de-duplicate successful operations because
                    // failed operations don't have instances to compare.
                    return;
                }

                var duplicates = Requests.Where(dup =>

                    // Successful requests where IDs are different
                    dup.Id != sourceId && dup.Success &&

                    // Instance is exactly the same
                    dup.Instance is object && object.ReferenceEquals(dup.Instance, source.Instance) &&

                    // Decorator target must also be the same (otherwise we lose the instance/decorator relationship)
                    dup.DecoratorTarget == source.DecoratorTarget).ToArray();
                if (duplicates.Length == 0)
                {
                    // No duplicates.
                    return;
                }

                foreach (var duplicate in duplicates)
                {
                    Requests.Remove(duplicate.Id);
                    foreach (var request in Requests)
                    {
                        var duplicateEdges = request.Edges.Where(e => e.Request == duplicate.Id).ToArray();
                        foreach (var duplicateEdge in duplicateEdges)
                        {
                            // Replace edges pointing to the duplicate so they
                            // point at the new source. HashSet will only keep
                            // unique edges, so if there was already a link to
                            // the source, there won't be duplicate edges.
                            // Also, duplicateEdge will never be null but the
                            // analyzer thinks it could be in that GraphEdge.ctor
                            // call.
                            request.Edges.Remove(duplicateEdge);
                            request.Edges.Add(new GraphEdge(sourceId, duplicateEdge!.Service));
                            if (!source.Services.ContainsKey(duplicateEdge.Service))
                            {
                                source.Services.Add(duplicateEdge.Service, Guid.NewGuid());
                            }
                        }
                    }
                }
            }

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.AppendLine("digraph G {");
                Operation.ToString(builder);
                foreach (var request in Requests)
                {
                    request.ToString(builder, Requests);
                }

                builder.AppendLine("}");
                return builder.ToString();
            }
        }
    }
}
