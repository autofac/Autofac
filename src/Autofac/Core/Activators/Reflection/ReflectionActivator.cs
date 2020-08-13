// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac.Builder;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Uses reflection to activate instances of a type.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "There is nothing in the derived class to dispose so no override is necessary.")]
    public class ReflectionActivator : InstanceActivator, IInstanceActivator
    {
        private readonly Type _implementationType;
        private readonly Parameter[] _configuredProperties;
        private readonly Parameter[] _defaultParameters;

        private ConstructorBinder[]? _constructorBinders;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionActivator"/> class.
        /// </summary>
        /// <param name="implementationType">Type to activate.</param>
        /// <param name="constructorFinder">Constructor finder.</param>
        /// <param name="constructorSelector">Constructor selector.</param>
        /// <param name="configuredParameters">Parameters configured explicitly for this instance.</param>
        /// <param name="configuredProperties">Properties configured explicitly for this instance.</param>
        public ReflectionActivator(
            Type implementationType,
            IConstructorFinder constructorFinder,
            IConstructorSelector constructorSelector,
            IEnumerable<Parameter> configuredParameters,
            IEnumerable<Parameter> configuredProperties)
            : base(implementationType)
        {
            if (configuredParameters == null)
            {
                throw new ArgumentNullException(nameof(configuredParameters));
            }

            if (configuredProperties == null)
            {
                throw new ArgumentNullException(nameof(configuredProperties));
            }

            _implementationType = implementationType;
            ConstructorFinder = constructorFinder ?? throw new ArgumentNullException(nameof(constructorFinder));
            ConstructorSelector = constructorSelector ?? throw new ArgumentNullException(nameof(constructorSelector));
            _configuredProperties = configuredProperties.ToArray();
            _defaultParameters = configuredParameters.Concat(new Parameter[] { new AutowiringParameter(), new DefaultValueParameter() }).ToArray();
        }

        /// <summary>
        /// Gets the constructor finder.
        /// </summary>
        public IConstructorFinder ConstructorFinder { get; }

        /// <summary>
        /// Gets the constructor selector.
        /// </summary>
        public IConstructorSelector ConstructorSelector { get; }

        /// <inheritdoc/>
        public void ConfigurePipeline(IComponentRegistryServices componentRegistryServices, IResolvePipelineBuilder pipelineBuilder)
        {
            if (componentRegistryServices is null)
            {
                throw new ArgumentNullException(nameof(componentRegistryServices));
            }

            if (pipelineBuilder is null)
            {
                throw new ArgumentNullException(nameof(pipelineBuilder));
            }

            // Locate the possible constructors at container build time.
            var availableConstructors = ConstructorFinder.FindConstructors(_implementationType);

            if (availableConstructors.Length == 0)
            {
                throw new NoConstructorsFoundException(_implementationType, string.Format(CultureInfo.CurrentCulture, ReflectionActivatorResources.NoConstructorsAvailable, _implementationType, ConstructorFinder));
            }

            var binders = new ConstructorBinder[availableConstructors.Length];

            for (var idx = 0; idx < availableConstructors.Length; idx++)
            {
                binders[idx] = new ConstructorBinder(availableConstructors[idx]);
            }

            _constructorBinders = binders;

            pipelineBuilder.Use(ToString(), PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (ctxt, next) =>
            {
                ctxt.Instance = ActivateInstance(ctxt, ctxt.Parameters);

                next(ctxt);
            });
        }

        /// <summary>
        /// Activate an instance in the provided context.
        /// </summary>
        /// <param name="context">Context in which to activate instances.</param>
        /// <param name="parameters">Parameters to the instance.</param>
        /// <returns>The activated instance.</returns>
        /// <remarks>
        /// The context parameter here should probably be ILifetimeScope in order to reveal Disposer,
        /// but will wait until implementing a concrete use case to make the decision.
        /// </remarks>
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

            var allBindings = GetAllBindings(_constructorBinders!, context, parameters);

            var selectedBinding = ConstructorSelector.SelectConstructorBinding(allBindings, parameters);

            if (!selectedBinding.CanInstantiate)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    ReflectionActivatorResources.ConstructorSelectorCannotSelectAnInvalidBinding,
                    ConstructorSelector.GetType().Name));
            }

            var instance = selectedBinding.Instantiate();

            InjectProperties(instance, context);

            return instance;
        }

        private BoundConstructor[] GetAllBindings(ConstructorBinder[] availableConstructors, IComponentContext context, IEnumerable<Parameter> parameters)
        {
            // Most often, there will be no `parameters` and/or no `_defaultParameters`; in both of those cases we can avoid allocating.
            var prioritisedParameters = parameters.Any() ? EnumerateParameters(parameters) : _defaultParameters;

            var boundConstructors = new BoundConstructor[availableConstructors.Length];
            var validBindings = availableConstructors.Length;

            for (var idx = 0; idx < availableConstructors.Length; idx++)
            {
                var bound = availableConstructors[idx].Bind(prioritisedParameters, context);

                boundConstructors[idx] = bound;

                if (!bound.CanInstantiate)
                {
                    validBindings--;
                }
            }

            if (validBindings == 0)
            {
                throw new DependencyResolutionException(GetBindingFailureMessage(boundConstructors));
            }

            return boundConstructors;
        }

        private IEnumerable<Parameter> EnumerateParameters(IEnumerable<Parameter> parameters)
        {
            foreach (var param in parameters)
            {
                yield return param;
            }

            foreach (var defaultParam in _defaultParameters)
            {
                yield return defaultParam;
            }
        }

        private string GetBindingFailureMessage(BoundConstructor[] constructorBindings)
        {
            var reasons = new StringBuilder();

            foreach (var invalid in constructorBindings.Where(cb => !cb.CanInstantiate))
            {
                reasons.AppendLine();
                reasons.Append(invalid.Description);
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                ReflectionActivatorResources.NoConstructorsBindable,
                ConstructorFinder,
                _implementationType,
                reasons);
        }

        private void InjectProperties(object instance, IComponentContext context)
        {
            if (_configuredProperties.Length == 0)
            {
                return;
            }

            var actualProperties = instance
                .GetType()
                .GetRuntimeProperties()
                .Where(pi => pi.CanWrite)
                .ToList();

            foreach (var configuredProperty in _configuredProperties)
            {
                foreach (var actualProperty in actualProperties)
                {
                    var setter = actualProperty.SetMethod;

                    if (setter != null &&
                        configuredProperty.CanSupplyValue(setter.GetParameters()[0], context, out var vp))
                    {
                        actualProperties.Remove(actualProperty);
                        actualProperty.SetValue(instance, vp(), null);
                        break;
                    }
                }
            }
        }
    }
}
