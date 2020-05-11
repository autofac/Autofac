using System;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Pipeline
{
    /// <summary>
    /// Encapsulates the call back that represents the entry point of a pipeline.
    /// </summary>
    internal class ResolvePipeline : IResolvePipeline
    {
        private readonly Action<IResolveRequestContext>? _entryPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvePipeline"/> class.
        /// </summary>
        /// <param name="entryPoint">Callback to invoke.</param>
        public ResolvePipeline(Action<IResolveRequestContext>? entryPoint)
        {
            _entryPoint = entryPoint;
        }

        /// <inheritdoc />
        public void Invoke(IResolveRequestContext ctxt)
        {
            _entryPoint?.Invoke(ctxt);
        }
    }
}
