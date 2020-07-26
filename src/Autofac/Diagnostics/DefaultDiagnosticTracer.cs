// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Autofac.Core;
using Autofac.Core.Resolving;

namespace Autofac.Diagnostics
{
    /// <summary>
    /// Provides a default resolve pipeline tracer that builds a multi-line
    /// string describing the end-to-end operation flow. Attach to the
    /// <see cref="OperationDiagnosticTracerBase{TContent}.OperationCompleted"/>
    /// event to receive notifications when new trace content is available.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tracer subscribes to all Autofac diagnostic events and can't be
    /// unsubscribed. This is required to ensure beginning and end of each
    /// logical activity can be captured.
    /// </para>
    /// </remarks>
    public class DefaultDiagnosticTracer : OperationDiagnosticTracerBase<string>
    {
        private const string RequestExceptionTraced = "__RequestException";

        private readonly ConcurrentDictionary<IResolveOperation, IndentingStringBuilder> _operationBuilders = new ConcurrentDictionary<IResolveOperation, IndentingStringBuilder>();

        private static readonly string[] NewLineSplit = new[] { Environment.NewLine };

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDiagnosticTracer"/> class.
        /// </summary>
        public DefaultDiagnosticTracer()
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
        protected override void OnOperationStart(OperationStartDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            var builder = _operationBuilders.GetOrAdd(data.Operation, k => new IndentingStringBuilder());

            builder.AppendFormattedLine(TracerMessages.ResolveOperationStarting);
            builder.AppendLine(TracerMessages.EntryBrace);
            builder.Indent();
        }

        /// <inheritdoc/>
        protected override void OnRequestStart(RequestDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
            {
                builder.AppendFormattedLine(TracerMessages.ResolveRequestStarting);
                builder.AppendLine(TracerMessages.EntryBrace);
                builder.Indent();
                builder.AppendFormattedLine(TracerMessages.ServiceDisplay, data.RequestContext.Service);
                builder.AppendFormattedLine(TracerMessages.ComponentDisplay, data.RequestContext.Registration.Activator.DisplayName());

                if (data.RequestContext.DecoratorTarget is object)
                {
                    builder.AppendFormattedLine(TracerMessages.TargetDisplay, data.RequestContext.DecoratorTarget.Activator.DisplayName());
                }

                builder.AppendLine();
                builder.AppendLine(TracerMessages.Pipeline);
            }
        }

        /// <inheritdoc/>
        protected override void OnMiddlewareStart(MiddlewareDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.RequestContext.Operation, out var builder))
            {
                builder.AppendFormattedLine(TracerMessages.EnterMiddleware, data.Middleware.ToString());
                builder.Indent();
            }
        }

        /// <inheritdoc/>
        protected override void OnMiddlewareFailure(MiddlewareDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.RequestContext.Operation, out var builder))
            {
                builder.Outdent();
                builder.AppendFormattedLine(TracerMessages.ExitMiddlewareFailure, data.Middleware.ToString());
            }
        }

        /// <inheritdoc/>
        protected override void OnMiddlewareSuccess(MiddlewareDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.RequestContext.Operation, out var builder))
            {
                builder.Outdent();
                builder.AppendFormattedLine(TracerMessages.ExitMiddlewareSuccess, data.Middleware.ToString());
            }
        }

        /// <inheritdoc/>
        protected override void OnRequestFailure(RequestFailureDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
            {
                builder.Outdent();
                builder.AppendLine(TracerMessages.ExitBrace);
                var requestException = data.RequestException;

                if (requestException is DependencyResolutionException && requestException.InnerException is object)
                {
                    requestException = requestException.InnerException;
                }

                if (requestException.Data.Contains(RequestExceptionTraced))
                {
                    builder.AppendLine(TracerMessages.ResolveRequestFailedNested);
                }
                else
                {
                    builder.AppendException(TracerMessages.ResolveRequestFailed, requestException);
                }

                requestException.Data[RequestExceptionTraced] = true;
            }
        }

        /// <inheritdoc/>
        protected override void OnRequestSuccess(RequestDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
            {
                builder.Outdent();
                builder.AppendLine(TracerMessages.ExitBrace);
                builder.AppendFormattedLine(TracerMessages.ResolveRequestSucceeded, data.RequestContext.Instance);
            }
        }

        /// <inheritdoc/>
        protected override void OnOperationFailure(OperationFailureDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
            {
                try
                {
                    builder.Outdent();
                    builder.AppendLine(TracerMessages.ExitBrace);
                    builder.AppendException(TracerMessages.OperationFailed, data.OperationException);

                    OnOperationCompleted(new OperationTraceCompletedArgs<string>(data.Operation, builder.ToString()));
                }
                finally
                {
                    _operationBuilders.TryRemove(data.Operation, out var _);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnOperationSuccess(OperationSuccessDiagnosticData data)
        {
            if (data is null)
            {
                return;
            }

            if (_operationBuilders.TryGetValue(data.Operation, out var builder))
            {
                try
                {
                    builder.Outdent();
                    builder.AppendLine(TracerMessages.ExitBrace);
                    builder.AppendFormattedLine(TracerMessages.OperationSucceeded, data.ResolvedInstance);

                    OnOperationCompleted(new OperationTraceCompletedArgs<string>(data.Operation, builder.ToString()));
                }
                finally
                {
                    _operationBuilders.TryRemove(data.Operation, out var _);
                }
            }
        }

        /// <summary>
        /// Provides a string builder that auto-indents lines.
        /// </summary>
        private class IndentingStringBuilder
        {
            private const int IndentSize = 2;

            private readonly StringBuilder _builder;
            private int _indentCount;

            public IndentingStringBuilder()
            {
                _builder = new StringBuilder();
            }

            public void Indent()
            {
                _indentCount++;
            }

            public void Outdent()
            {
                if (_indentCount == 0)
                {
                    throw new InvalidOperationException(TracerMessages.OutdentFailure);
                }

                _indentCount--;
            }

            public void AppendFormattedLine(string format, params object?[] args)
            {
                AppendIndent();
                _builder.AppendFormat(CultureInfo.CurrentCulture, format, args);
                _builder.AppendLine();
            }

            public void AppendException(string message, Exception ex)
            {
                AppendIndent();
                _builder.AppendLine(message);

                var exceptionBody = ex.ToString().Split(NewLineSplit, StringSplitOptions.None);

                Indent();

                foreach (var exceptionLine in exceptionBody)
                {
                    AppendLine(exceptionLine);
                }

                Outdent();
            }

            public void AppendLine()
            {
                // No indent on a blank line.
                _builder.AppendLine();
            }

            public void AppendLine(string value)
            {
                AppendIndent();
                _builder.AppendLine(value);
            }

            private void AppendIndent()
            {
                _builder.Append(' ', IndentSize * _indentCount);
            }

            public override string ToString()
            {
                return _builder.ToString();
            }
        }
    }
}
