// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Resolving.Middleware
{
    /// <summary>
    /// Provides middleware to wrap propagating activator exceptions.
    /// </summary>
    internal class ActivatorErrorHandlingMiddleware : IResolveMiddleware
    {
        private const string ActivatorChainExceptionData = "ActivatorChain";

        /// <summary>
        /// Gets a singleton instance of the middleware.
        /// </summary>
        public static ActivatorErrorHandlingMiddleware Instance { get; } = new ActivatorErrorHandlingMiddleware();

        private ActivatorErrorHandlingMiddleware()
        {
        }

        /// <inheritdoc />
        public PipelinePhase Phase => PipelinePhase.Activation;

        /// <inheritdoc />
        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            try
            {
                next(context);

                if (context.Instance is null)
                {
                    // Exited the Activation Stage without creating an instance.
                    throw new DependencyResolutionException(MiddlewareMessages.ActivatorDidNotPopulateInstance);
                }
            }
            catch (ObjectDisposedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw PropagateActivationException(context.Registration.Activator, ex);
            }
        }

        private static DependencyResolutionException PropagateActivationException(IInstanceActivator activator, Exception exception)
        {
            var activatorChain = activator.DisplayName();
            var innerException = exception;

            if (exception.Data.Contains(ActivatorChainExceptionData) &&
                exception.Data[ActivatorChainExceptionData] is string innerChain)
            {
                activatorChain = activatorChain + " -> " + innerChain;
                innerException = exception.InnerException;
            }

            var result = new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ComponentActivationResources.ErrorDuringActivation, activatorChain), innerException);
            result.Data[ActivatorChainExceptionData] = activatorChain;
            return result;
        }

        /// <inheritdoc />
        public override string ToString() => nameof(ActivatorErrorHandlingMiddleware);
    }
}
