using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Autofac.Core.Resolving;

namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Provides a default resolve pipeline tracer that builds a multi-line string describing the end-to-end operation flow.
    /// Attach to the <see cref="OperationCompleted"/> event to receive notifications when new trace content is available.
    /// </summary>
    public class DefaultDiagnosticTracer : DiagnosticTracerBase
    {
        private const string RequestExceptionTraced = "__RequestException";

        private readonly ConcurrentDictionary<ITracingIdentifer, IndentingStringBuilder> _operationBuilders = new ConcurrentDictionary<ITracingIdentifer, IndentingStringBuilder>();

        private static readonly string[] NewLineSplit = new[] { Environment.NewLine };

        public DefaultDiagnosticTracer()
        {
            EnableAll();
        }

        public override void Enable(string diagnosticName)
        {
            // Do nothing. Default is always enabled for everything.
        }

        public override void Disable(string diagnosticName)
        {
            // Do nothing. Default is always enabled for everything.
        }

        /// <summary>
        /// Event raised when a resolve operation completes, and trace data is available.
        /// </summary>
        public event EventHandler<OperationTraceCompletedArgs>? OperationCompleted;

        /// <summary>
        /// Gets the number of operations in progress being traced.
        /// </summary>
        /// <value>
        /// An <see cref="int"/> with the number of trace IDs associated
        /// with in-progress operations being traced by this tracer.
        /// </value>
        public int OperationsInProgress => _operationBuilders.Count;

        /// <inheritdoc/>
        public override void OnOperationStart(OperationStartDiagnosticData data)
        {
            var builder = _operationBuilders.GetOrAdd(data.Operation.TracingId, k => new IndentingStringBuilder());

            builder.AppendFormattedLine(TracerMessages.ResolveOperationStarting);
            builder.AppendLine(TracerMessages.EntryBrace);
            builder.Indent();
        }

        /// <inheritdoc/>
        public override void OnRequestStart(RequestDiagnosticData data)
        {
            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
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
        public override void OnMiddlewareEntry(MiddlewareDiagnosticData data)
        {
            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                builder.AppendFormattedLine(TracerMessages.EnterMiddleware, data.Middleware.ToString());
                builder.Indent();
            }
        }

        /// <inheritdoc/>
        public override void OnMiddlewareFailure(MiddlewareDiagnosticData data)
        {
            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                builder.Outdent();
                builder.AppendFormattedLine(TracerMessages.ExitMiddlewareFailure, data.Middleware.ToString());
            }
        }

        /// <inheritdoc/>
        public override void OnMiddlewareSuccess(MiddlewareDiagnosticData data)
        {
            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                builder.Outdent();
                builder.AppendFormattedLine(TracerMessages.ExitMiddlewareSuccess, data.Middleware.ToString());
            }
        }

        /// <inheritdoc/>
        public override void OnRequestFailure(RequestFailureDiagnosticData data)
        {
            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
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
        public override void OnRequestSuccess(RequestDiagnosticData data)
        {
            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                builder.Outdent();
                builder.AppendLine(TracerMessages.ExitBrace);
                builder.AppendFormattedLine(TracerMessages.ResolveRequestSucceeded, data.RequestContext.Instance);
            }
        }

        /// <inheritdoc/>
        public override void OnOperationFailure(OperationFailureDiagnosticData data)
        {
            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                try
                {
                    builder.Outdent();
                    builder.AppendLine(TracerMessages.ExitBrace);
                    builder.AppendException(TracerMessages.OperationFailed, data.OperationException);

                    // If we're completing the root operation, raise the event.
                    if (data.Operation.IsTopLevelOperation)
                    {
                        OperationCompleted?.Invoke(this, new OperationTraceCompletedArgs(data.Operation, builder.ToString()));
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
            if (_operationBuilders.TryGetValue(data.Operation.TracingId, out var builder))
            {
                try
                {
                    builder.Outdent();
                    builder.AppendLine(TracerMessages.ExitBrace);
                    builder.AppendFormattedLine(TracerMessages.OperationSucceeded, data.ResolvedInstance);

                    // If we're completing the root operation, raise the event.
                    if (data.Operation.IsTopLevelOperation)
                    {
                        OperationCompleted?.Invoke(this, new OperationTraceCompletedArgs(data.Operation, builder.ToString()));
                    }
                }
                finally
                {
                    _operationBuilders.TryRemove(data.Operation.TracingId, out var _);
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
