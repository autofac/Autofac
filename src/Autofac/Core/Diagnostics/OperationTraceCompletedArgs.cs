using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Diagnostics
{
    public sealed class OperationTraceCompletedArgs
    {
        public OperationTraceCompletedArgs(ResolveOperationBase operation, string traceContent)
        {
            Operation = operation;
            TraceContent = traceContent;
        }

        public ResolveOperationBase Operation { get; }

        public string TraceContent { get; }
    }
}
