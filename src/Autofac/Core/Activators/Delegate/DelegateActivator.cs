// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Activators.Delegate
{
    /// <summary>
    /// Activate instances using a delegate.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "There is nothing in the derived class to dispose so no override is necessary.")]
    public class DelegateActivator : InstanceActivator, IInstanceActivator
    {
        private readonly Func<IComponentContext, IEnumerable<Parameter>, object> _activationFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateActivator"/> class.
        /// </summary>
        /// <param name="limitType">The most specific type to which activated instances can be cast.</param>
        /// <param name="activationFunction">Activation delegate.</param>
        public DelegateActivator(Type limitType, Func<IComponentContext, IEnumerable<Parameter>, object> activationFunction)
            : base(limitType)
        {
            _activationFunction = activationFunction ?? throw new ArgumentNullException(nameof(activationFunction));
        }

        /// <inheritdoc/>
        public void ConfigurePipeline(IComponentRegistryServices componentRegistryServices, IResolvePipelineBuilder pipelineBuilder)
        {
            if (pipelineBuilder is null)
            {
                throw new ArgumentNullException(nameof(pipelineBuilder));
            }

            pipelineBuilder.Use(this.DisplayName(), PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (ctxt, next) =>
            {
                ctxt.Instance = ActivateInstance(ctxt, ctxt.Parameters);

                next(ctxt);
            });
        }

        /// <summary>
        /// Invokes the delegate and returns the instance.
        /// </summary>
        /// <param name="context">Context in which to activate instances.</param>
        /// <param name="parameters">Parameters to the instance.</param>
        /// <returns>The activated instance.</returns>
        private object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            CheckNotDisposed();

            var result = _activationFunction(context, parameters);
            if (result == null)
            {
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, DelegateActivatorResources.NullFromActivationDelegateFor, LimitType));
            }

            return result;
        }
    }
}
