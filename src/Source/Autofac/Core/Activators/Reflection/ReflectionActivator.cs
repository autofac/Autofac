// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
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
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Uses reflection to activate instances of a type.
    /// </summary>
    public class ReflectionActivator : InstanceActivator, IInstanceActivator
    {
        readonly Type _implementationType;
        readonly IConstructorSelector _constructorSelector;
        readonly IConstructorFinder _constructorFinder;
        readonly IEnumerable<Parameter> _configuredParameters;
        readonly IEnumerable<Parameter> _configuredProperties;

        /// <summary>
        /// Create an activator for the provided type.
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
            : base(Enforce.ArgumentNotNull(implementationType, "implementationType"))
        {
            _implementationType = implementationType;
            _constructorFinder = Enforce.ArgumentNotNull(constructorFinder, "constructorFinder");
            _constructorSelector = Enforce.ArgumentNotNull(constructorSelector, "constructorSelector");
            _configuredParameters = Enforce.ArgumentNotNull(configuredParameters, "configuredParameters");
            _configuredProperties = Enforce.ArgumentNotNull(configuredProperties, "configuredProperties");
        }

        /// <summary>
        /// The constructor finder.
        /// </summary>
        public IConstructorFinder ConstructorFinder
        {
            get { return _constructorFinder; }
        }

        /// <summary>
        /// The constructor selector.
        /// </summary>
        public IConstructorSelector ConstructorSelector
        {
            get { return _constructorSelector; }
        }

        /// <summary>
        /// Activate an instance in the provided context.
        /// </summary>
        /// <param name="context">Context in which to activate instances.</param>
        /// <param name="parameters">Parameters to the instance.</param>
        /// <returns>The activated instance.</returns>
        /// <remarks>
        /// The context parameter here should probably be ILifetimeScope in order to reveal Disposer,
        /// but will wait until implementing a concrete use case to make the decision
        /// </remarks>
        public object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

            var availableConstructors = _constructorFinder.FindConstructors(_implementationType);

            if (!availableConstructors.Any())
                throw new DependencyResolutionException(ReflectionActivatorResources.NoConstructorsAvailable);

            var constructorBindings = GetConstructorBindings(
                context,
                parameters,
                availableConstructors);

            var validBindings = constructorBindings.Where(cb => cb.CanInstantiate);

            if (!validBindings.Any())
                throw new DependencyResolutionException(GetBindingFailureMessage(constructorBindings));

            var selectedBinding = _constructorSelector.SelectConstructorBinding(validBindings);

            var instance = selectedBinding.Instantiate();

            InjectProperties(instance, context);

            return instance;
        }

        string GetBindingFailureMessage(IEnumerable<ConstructorParameterBinding> constructorBindings)
        {
            var reasons = new StringBuilder();

            foreach (var invalid in constructorBindings.Where(cb => !cb.CanInstantiate))
            {
                reasons.AppendLine();
                reasons.Append(invalid.Description);
            }

            return string.Format(
                ReflectionActivatorResources.NoConstructorsBindable,
                _constructorFinder, _implementationType, reasons);
        }

        IEnumerable<ConstructorParameterBinding> GetConstructorBindings(
            IComponentContext context,
            IEnumerable<Parameter> parameters,
            IEnumerable<ConstructorInfo> constructorInfo)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");
            Enforce.ArgumentNotNull(constructorInfo, "constructorInfo");

            var prioritisedParameters =
                parameters.Concat(
                    _configuredParameters.Concat(
                        new Parameter[] { new AutowiringParameter() }));

            return constructorInfo
                .Select(ci => new ConstructorParameterBinding(ci, prioritisedParameters, context))
                .ToArray();
        }

        void InjectProperties(object instance, IComponentContext context)
        {
            if (_configuredProperties.Any())
            {
                var actualProps = instance
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance)
                    .ToList();

                foreach (var prop in _configuredProperties)
                {
                    Func<object> vp = null;
                    foreach (var actual in actualProps)
                    {
                        var setter = actual.GetSetMethod();
                        if (setter != null &&
                            prop.CanSupplyValue(setter.GetParameters().First(), context, out vp))
                        {
                            actualProps.Remove(actual);
                            actual.SetValue(instance, vp(), null);
                            break;
                        }
                    }
                }
            }
        }
    }
}
