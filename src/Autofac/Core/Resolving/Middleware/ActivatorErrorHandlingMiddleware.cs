// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
