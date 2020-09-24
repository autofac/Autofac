// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Represents a pipeline that can be invoked to resolve an instance of a service.
    /// </summary>
    public interface IResolvePipeline
    {
        /// <summary>
        /// Invoke the pipeline to the end, or until an exception is thrown.
        /// </summary>
        /// <param name="context">The request context.</param>
        void Invoke(ResolveRequestContext context);
    }
}
