// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Autofac.Component.Activation
{
    /// <summary>
    /// A component activator that creates a new instance of a
    /// type using reflection to select and invoke a constructor
    /// based on the available service registrations.
    /// </summary>
    public class ReflectionActivator : IActivator
	{
		Type _componentType;
		IDictionary<string, object> _additionalConstructorParameters = new Dictionary<string, object>();
		IDictionary<string, object> _explicitPropertySetters = new Dictionary<string, object>();
		IConstructorSelector _constructorSelector;
        IConstructorInvoker _constructorInvoker = new DirectConstructorInvoker();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionActivator"/> class.
        /// </summary>
        /// <param name="componentType">Type of the component.</param>
        public ReflectionActivator(Type componentType)
            : this(componentType, new Dictionary<string, object>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionActivator"/> class.
        /// </summary>
        /// <param name="componentType">Type of the component.</param>
        /// <param name="additionalConstructorParameters">The additional constructor parameters.</param>
		public ReflectionActivator(
			Type componentType,
			IDictionary<string, object> additionalConstructorParameters)
			: this(
				componentType,
				additionalConstructorParameters,
				new Dictionary<string, object>())
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionActivator"/> class.
        /// </summary>
        /// <param name="componentType">Type of the component.</param>
        /// <param name="additionalConstructorParameters">The additional constructor parameters.</param>
        /// <param name="explicitPropertySetters">The explicit property setters.</param>
		public ReflectionActivator(
			Type componentType,
			IDictionary<string, object> additionalConstructorParameters,
			IDictionary<string, object> explicitPropertySetters)
			: this(
				componentType,
				additionalConstructorParameters,
				explicitPropertySetters,
				new MostParametersConstructorSelector())
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionActivator"/> class.
        /// </summary>
        /// <param name="componentType">Type of the component.</param>
        /// <param name="additionalConstructorParameters">The additional constructor parameters.</param>
        /// <param name="explicitPropertySetters">The explicit property setters.</param>
        /// <param name="constructorSelector">The constructor selector.</param>
        public ReflectionActivator(
            Type componentType,
            IDictionary<string, object> additionalConstructorParameters,
			IDictionary<string, object> explicitPropertySetters,
            IConstructorSelector constructorSelector)
        {
            Enforce.ArgumentNotNull(componentType, "componentType");
            Enforce.ArgumentNotNull(additionalConstructorParameters, "additionalConstructorParameters");
            Enforce.ArgumentNotNull(explicitPropertySetters, "explicitPropertySetters");
            Enforce.ArgumentNotNull(constructorSelector, "constructorSelector");

            _componentType = componentType;
            _constructorSelector = constructorSelector;

			List<string> seenParameters = new List<string>();
			foreach (KeyValuePair<string, object> parameter in additionalConstructorParameters)
			{
				if (string.IsNullOrEmpty(parameter.Key))
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
						ReflectionActivatorResources.ParameterNameNotNullOrEmpty, componentType));

				if (seenParameters.Contains(parameter.Key))
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
							ReflectionActivatorResources.ParameterSpecifiedMultipleTimes, parameter.Key, componentType));

				seenParameters.Add(parameter.Key);

				_additionalConstructorParameters.Add(parameter);
			}

			List<string> seenProperties = new List<string>();
			foreach (KeyValuePair<string, object> property in explicitPropertySetters)
			{
				if (string.IsNullOrEmpty(property.Key))
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
						ReflectionActivatorResources.PropertyNameNotNullOrEmpty, componentType));

				if (seenProperties.Contains(property.Key))
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
							ReflectionActivatorResources.PropertySpecifiedMultipleTimes, property.Key, componentType));

				if (componentType.GetProperty(property.Key) == null)
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
							ReflectionActivatorResources.PropertyNotFound, property.Key, componentType));

				seenProperties.Add(property.Key);

				_explicitPropertySetters.Add(property);
			}
        }

        /// <summary>
        /// Create a component instance, using container
        /// to resolve the instance's dependencies.
        /// </summary>
        /// <param name="context">The context to use
        /// for dependency resolution.</param>
        /// <param name="parameters">Parameters that can be used in the resolution process.</param>
        /// <returns>
        /// A component instance. Note that while the
        /// returned value need not be created on-the-spot, it must
        /// not be returned more than once by consecutive calls. (Throw
        /// an exception if this is attempted. IActivationScope should
        /// manage singleton semantics.)
        /// </returns>
        public object ActivateInstance(IContext context, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

			var possibleConstructors = new List<ConstructorInfo>();
			StringBuilder reasons = null;
			bool foundPublicConstructor = false;

			foreach (ConstructorInfo ci in _componentType.FindMembers(
				MemberTypes.Constructor,
				BindingFlags.Instance | BindingFlags.Public,
				null,
				null))
			{
				foundPublicConstructor = true;
				
				string reason;
				if (CanUseConstructor(ci, context, parameters, out reason))
				{
					possibleConstructors.Add(ci);
				}
				else
				{
    				reasons = reasons ?? new StringBuilder(reason.Length + 2);
					reasons.AppendLine();
					reasons.Append(reason);
				}
			}

			if (!foundPublicConstructor)
				throw new DependencyResolutionException(
					string.Format(CultureInfo.CurrentCulture,
						ReflectionActivatorResources.NoPublicConstructor, _componentType));

            if (possibleConstructors.Count == 0)
    			throw new DependencyResolutionException(
						  string.Format(CultureInfo.CurrentCulture, ReflectionActivatorResources.NoResolvableConstructor, _componentType, reasons ?? new StringBuilder()));

            return ConstructInstance(
                _constructorSelector.SelectConstructor(possibleConstructors),
                context, parameters);
		}

        /// <summary>
        /// A 'new context' is a scope that is self-contained
        /// and that can dispose the components it contains before the parent
        /// container is disposed. If the activator is stateless it should return
        /// true, otherwise false.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can support a new context; otherwise, <c>false</c>.
        /// </value>
		public bool CanSupportNewContext
		{
			get
			{
				return true;
			}
		}

        /// <summary>
        /// The type that will be used to reflectively instantiate the component instances.
        /// </summary>
        /// <remarks>
        /// The actual implementation type may be substituted with a dynamically-generated subclass.
        /// Note that functionality that  relies on this feature will obviously not be available to provided instances or
        /// to delegate-created instances; interface-based AOP is recommended in these situations.
        /// </remarks>
        public Type ImplementationType
        {
            get
            {
                return _componentType;
            }
            set
            {
                _componentType = Enforce.ArgumentNotNull(value, "value");
            }
        }

        /// <summary>
        /// Gets or sets the constructor invoker.
        /// </summary>
        /// <value>The constructor invoker.</value>
        public IConstructorInvoker ConstructorInvoker
        {
            get
            {
                return _constructorInvoker;
            }
            set
            {
                _constructorInvoker = Enforce.ArgumentNotNull(value, "value");
            }
        }

        bool CanUseConstructor(ConstructorInfo ci, IContext context, IActivationParameters parameters, out string reason)
        {
            Enforce.ArgumentNotNull(ci, "ci");
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

            StringBuilder reasonNotUsable = null;

            foreach (ParameterInfo pi in ci.GetParameters())
            {
                if (!context.IsRegistered(pi.ParameterType) &&
                    !parameters.ContainsKey(pi.Name) &&
                    !_additionalConstructorParameters.ContainsKey(pi.Name))
                {
                    if (reasonNotUsable == null)
                    {
                        reasonNotUsable = new StringBuilder();
                        reasonNotUsable.Append(ci).Append(": ");
                    }
                    else
                    {
                        reasonNotUsable.Append(", ");
                    }

                    reasonNotUsable.AppendFormat(CultureInfo.CurrentCulture,
                        ReflectionActivatorResources.MissingParameter,
                        pi.Name, pi.ParameterType);
                }
            }

            if (reasonNotUsable != null)
                reason = reasonNotUsable.Append('.').ToString();
            else
                reason = String.Empty;

            // Return true if there is no reason not to use it, i.e. reasonNotUsable is null
            return reasonNotUsable == null;
        }

        /// <summary>
        /// Dependencies must be resolvable. Check this first with AreAllParametersRegistered().
        /// </summary>
        /// <param name="ci">The constructor to use.</param>
        /// <param name="context">The context in which the instance is being created.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The new instance.</returns>
        /// <exception cref="DependencyResolutionException">Parameters were not resolvable.</exception>
        object ConstructInstance(ConstructorInfo ci, IContext context, IActivationParameters parameters)
        {
            Enforce.ArgumentNotNull(ci, "ci");
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

            ParameterInfo[] ciParameters = ci.GetParameters();
            var parameterValues = new object[ciParameters.Length];
            int parameterIndex = 0;
            foreach (ParameterInfo pi in ciParameters)
            {
                object parameterValue;
                if (!parameters.TryGetValue(pi.Name, out parameterValue) &&
                    !_additionalConstructorParameters.TryGetValue(pi.Name, out parameterValue))
                {
                    parameterValue = context.Resolve(pi.ParameterType);
                }

                parameterValues[parameterIndex++] = TypeManipulation.ChangeToCompatibleType(parameterValue, pi.ParameterType);
            }

            try
            {
                object instance = ConstructorInvoker.InvokeConstructor(context, parameters, ci, parameterValues);
                SetterInject(instance, context);
                return instance;
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

    	/// <summary>
		/// Inspect all public writeable properties and inject
		/// values from the container if available. For factory-lifecycle components
		/// a speed improvement could be had here by caching the property-value
		/// pairs.
		/// </summary>
		/// <param name="instance">The non-null object instance to perform
		/// setter injection on.</param>
		/// <param name="context">The non-null context from which dependencies
		/// may be satisfied.</param>
		void SetterInject(object instance, IContext context)
		{
            Enforce.ArgumentNotNull(instance, "instance");
            Enforce.ArgumentNotNull(context, "context");

            if (_explicitPropertySetters.Count == 0)
                return;

			Type instanceType = instance.GetType();

			foreach (PropertyInfo property in instanceType.GetProperties(
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
			{
				Type propertyType = property.PropertyType;

				object propertyValue;
				if (_explicitPropertySetters.TryGetValue(property.Name, out propertyValue))
				{
                    propertyValue = TypeManipulation.ChangeToCompatibleType(propertyValue, propertyType);
					property.SetValue(instance, propertyValue, null);
				}
			}
		}

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return ReflectionActivatorResources.Description;
        }
	}
}
