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
using System.Linq;

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
    	readonly IEnumerable<Parameter> _additionalConstructorParameters = Enumerable.Empty<Parameter>();
    	readonly IEnumerable<NamedPropertyParameter> _explicitPropertySetters = Enumerable.Empty<NamedPropertyParameter>();
    	readonly IConstructorSelector _constructorSelector;
        static readonly IConstructorInvoker DirectInvoker = new DirectConstructorInvoker();
        IConstructorInvoker _constructorInvoker = DirectInvoker;
        static readonly IEnumerable<Parameter> AutowiringParameterArray = new Parameter[] { new AutowiringParameter() };
    	static readonly MethodInfo InternalPreserveStackTraceMethod = typeof (Exception)
    		.GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionActivator"/> class.
        /// </summary>
        /// <param name="componentType">Type of the component.</param>
        public ReflectionActivator(Type componentType)
            : this(componentType, Enumerable.Empty<Parameter>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionActivator"/> class.
        /// </summary>
        /// <param name="componentType">Type of the component.</param>
        /// <param name="additionalConstructorParameters">The additional constructor parameters.</param>
		public ReflectionActivator(
			Type componentType,
			IEnumerable<Parameter> additionalConstructorParameters)
			: this(
				componentType,
				additionalConstructorParameters,
                Enumerable.Empty<NamedPropertyParameter>())
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
            IEnumerable<Parameter> additionalConstructorParameters,
            IEnumerable<NamedPropertyParameter> explicitPropertySetters)
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
            IEnumerable<Parameter> additionalConstructorParameters,
            IEnumerable<NamedPropertyParameter> explicitPropertySetters,
            IConstructorSelector constructorSelector)
        {
            Enforce.ArgumentNotNull(componentType, "componentType");
            Enforce.ArgumentNotNull(additionalConstructorParameters, "additionalConstructorParameters");
            Enforce.ArgumentNotNull(explicitPropertySetters, "explicitPropertySetters");
            Enforce.ArgumentNotNull(constructorSelector, "constructorSelector");

            _componentType = componentType;
            _constructorSelector = constructorSelector;
            _additionalConstructorParameters = additionalConstructorParameters.Concat(AutowiringParameterArray).ToArray();
            _explicitPropertySetters = explicitPropertySetters.ToArray();
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
        public object ActivateInstance(IContext context, IEnumerable<Parameter> parameters)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

			var possibleConstructors = new Dictionary<ConstructorInfo, Func<object>[]>();
			StringBuilder reasons = null;
			bool foundPublicConstructor = false;
            var augmentedParameters = parameters.Concat(_additionalConstructorParameters);

			foreach (ConstructorInfo ci in _componentType.FindMembers(
				MemberTypes.Constructor,
				BindingFlags.Instance | BindingFlags.Public,
				null,
				null))
			{
				foundPublicConstructor = true;

                Func<object>[] parameterAccessors;
				string reason;
				if (CanUseConstructor(ci, context, augmentedParameters, out parameterAccessors, out reason))
				{
					possibleConstructors.Add(ci, parameterAccessors);
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

            var selectedCI = _constructorSelector.SelectConstructor(possibleConstructors.Keys);

            var result = ConstructInstance(selectedCI, context, augmentedParameters, possibleConstructors[selectedCI]);

            SetExplicitProperties(result, context);

            return result;
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

        bool CanUseConstructor(ConstructorInfo ci, IContext context, IEnumerable<Parameter> parameters, out Func<object>[] valueAccessors, out string reason)
        {
            Enforce.ArgumentNotNull(ci, "ci");
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

            StringBuilder reasonNotUsable = null;

            var ciParams = ci.GetParameters();
            var partialValueAccessors = new Func<object>[ciParams.Length];

            foreach (ParameterInfo pi in ciParams)
            {
                Func<object> va = null;

                foreach (var param in parameters)
                {
                    if (param.CanSupplyValue(pi, context, out va))
                        break;
                }

                if (va != null)
                {
                    partialValueAccessors[pi.Position] = va;
                }
                else
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
            {
                valueAccessors = null;
                reason = reasonNotUsable.Append('.').ToString();
            }
            else
            {
                valueAccessors = partialValueAccessors;
                reason = String.Empty;
            }

            // Return true if there is no reason not to use it, i.e. reasonNotUsable is null
            return reasonNotUsable == null;
        }

		static Exception UnwindInvocationException(TargetInvocationException ex)
		{
            Exception result = ex;

            if (ex.InnerException != null && InternalPreserveStackTraceMethod != null)
            {
                result = ex.InnerException;
                InternalPreserveStackTraceMethod.Invoke(result, null);
            }

			return result;
		}

        object ConstructInstance(ConstructorInfo ci, IContext context, IEnumerable<Parameter> parameters, Func<object>[] parameterAccessors)
        {
            Enforce.ArgumentNotNull(ci, "ci");
            Enforce.ArgumentNotNull(parameterAccessors, "parameterAccessors");

            try
            {
                object instance = ConstructorInvoker.InvokeConstructor(context, parameters, ci, parameterAccessors);
                return instance;
            }
            catch (TargetInvocationException tie)
            {
            	throw UnwindInvocationException(tie);
            }
        }

		void SetExplicitProperties(object instance, IContext context)
		{
            Enforce.ArgumentNotNull(instance, "instance");
            Enforce.ArgumentNotNull(context, "context");

            if (!_explicitPropertySetters.Any())
                return;

			Type instanceType = instance.GetType();

			// Rinat Abdullin: properties with signature like {private set;get;} pass the
			// BindingFlags.SetProperty but fail around "GetSetMethod()", since it returns null
			// for non-public properties
			var properties = instanceType.GetProperties(
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
				.Select(p => new
				{
					Info = p,
					SetMethod = p.GetSetMethod()
				})
				.Where(p => p.SetMethod != null)
				.ToArray();

            foreach (var param in _explicitPropertySetters)
            {
            	foreach (var propertyData in properties)
			    {
                    Func<object> propertyValueAccessor;
                    if (param.CanSupplyValue(propertyData.SetMethod.GetParameters()[0], context, out propertyValueAccessor))
                    {
                        propertyData.Info.SetValue(instance, propertyValueAccessor(), null);
                    }
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
