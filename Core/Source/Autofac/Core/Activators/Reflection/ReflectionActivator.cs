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
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Uses reflection to activate instances of a type.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "There is nothing in the derived class to dispose so no override is necessary.")]
    public class ReflectionActivator : InstanceActivator, IInstanceActivator
    {
        readonly Type _implementationType;
        readonly IConstructorSelector _constructorSelector;
        readonly IConstructorFinder _constructorFinder;
        readonly IEnumerable<Parameter> _configuredParameters;
        readonly IEnumerable<Parameter> _configuredProperties;
        readonly ConstructorInfo[] _availableConstructors;

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

            _availableConstructors = _constructorFinder.FindConstructors(_implementationType);

            if (_availableConstructors.Length == 0)
                throw new DependencyResolutionException(string.Format(
                    CultureInfo.CurrentCulture, ReflectionActivatorResources.NoConstructorsAvailable, _implementationType, _constructorFinder));
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
            if (context == null) throw new ArgumentNullException("context");
            if (parameters == null) throw new ArgumentNullException("parameters");

            var constructorBindings = GetConstructorBindings(
                context,
                parameters,
                _availableConstructors);

            var validBindings = constructorBindings
                .Where(cb => cb.CanInstantiate)
                .ToArray();

            if (validBindings.Length == 0)
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
                CultureInfo.CurrentCulture,
                ReflectionActivatorResources.NoConstructorsBindable,
                _constructorFinder, _implementationType, reasons);
        }

        IEnumerable<ConstructorParameterBinding> GetConstructorBindings(
            IComponentContext context,
            IEnumerable<Parameter> parameters,
            IEnumerable<ConstructorInfo> constructorInfo)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (parameters == null) throw new ArgumentNullException("parameters");
            if (constructorInfo == null) throw new ArgumentNullException("constructorInfo");

            var prioritisedParameters =
                parameters.Concat(
                    _configuredParameters.Concat(
                        new Parameter[] { new AutowiringParameter(), new DefaultValueParameter() }));

            return constructorInfo.Select(ci => new ConstructorParameterBinding(ci, prioritisedParameters, context));
        }

        void InjectProperties(object instance, IComponentContext context)
        {
            if (!_configuredProperties.Any())
                return;

            var actualProps = instance
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi => pi.CanWrite)
                .ToList();

            foreach (var prop in _configuredProperties)
            {
                foreach (var actual in actualProps)
                {
                    var setter = actual.GetSetMethod();
                    Func<object> vp;
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
