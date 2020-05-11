using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Diagnostics
{
    public sealed class OperationTraceCompletedArgs
    {
        public OperationTraceCompletedArgs(IPipelineResolveOperation operation, string traceContent)
        {
            Operation = operation;
            TraceContent = traceContent;
        }

        public IPipelineResolveOperation Operation { get; }

        public string TraceContent { get; }
    }
}
