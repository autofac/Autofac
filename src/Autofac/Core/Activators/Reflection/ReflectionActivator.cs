// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
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
        private ConstructorInfo[] _availableConstructors;
        private readonly object _availableConstructorsLock = new object();

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
            if (constructorFinder == null) throw new ArgumentNullException(nameof(constructorFinder));
            if (constructorSelector == null) throw new ArgumentNullException(nameof(constructorSelector));
            if (configuredParameters == null) throw new ArgumentNullException(nameof(configuredParameters));
            if (configuredProperties == null) throw new ArgumentNullException(nameof(configuredProperties));

            _implementationType = implementationType;
            ConstructorFinder = constructorFinder;
            ConstructorSelector = constructorSelector;
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
        public object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            // Lazy instantiate available constructor list so the constructor
            // finder can be changed during AsSelf() registration. AsSelf() creates
            // a temporary activator just long enough to get the LimitType.
            if (_availableConstructors == null)
            {
                lock (_availableConstructorsLock)
                {
                    if (_availableConstructors == null)
                    {
                        _availableConstructors = ConstructorFinder.FindConstructors(_implementationType);
                    }
                }
            }

            if (_availableConstructors.Length == 0)
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ReflectionActivatorResources.NoConstructorsAvailable, _implementationType, ConstructorFinder));

            var validBindings = GetValidConstructorBindings(context, parameters);

            var selectedBinding = ConstructorSelector.SelectConstructorBinding(validBindings, parameters);

            var instance = selectedBinding.Instantiate();

            InjectProperties(instance, context);

            return instance;
        }

        private ConstructorParameterBinding[] GetValidConstructorBindings(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            // Most often, there will be no `parameters` and/or no `_defaultParameters`; in both of those cases we can avoid allocating.
            var prioritisedParameters = parameters.Any() ?
                (_defaultParameters.Length == 0 ? parameters : parameters.Concat(_defaultParameters)) :
                _defaultParameters;

            var constructorBindings = new ConstructorParameterBinding[_availableConstructors.Length];
            for (var i = 0; i < _availableConstructors.Length; ++i)
            {
                constructorBindings[i] = new ConstructorParameterBinding(_availableConstructors[i], prioritisedParameters, context);
            }

            // Copy-on-write; 99% of components will have a single constructor that can be instantiated.
            var validBindings = constructorBindings;
            for (var i = 0; i < constructorBindings.Length; ++i)
            {
                if (!constructorBindings[i].CanInstantiate)
                {
                    // Further optimisation opportunity here
                    validBindings = constructorBindings
                        .Where(cb => cb.CanInstantiate)
                        .ToArray();

                    break;
                }
            }

            if (validBindings.Length == 0)
                throw new DependencyResolutionException(GetBindingFailureMessage(constructorBindings));

            return validBindings;
        }

        private string GetBindingFailureMessage(IEnumerable<ConstructorParameterBinding> constructorBindings)
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
                return;

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
                    Func<object> vp;
                    if (setter != null &&
                        configuredProperty.CanSupplyValue(setter.GetParameters().First(), context, out vp))
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
