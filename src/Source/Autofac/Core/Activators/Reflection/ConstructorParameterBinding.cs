using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Binds a constructor to the parameters that will be used when it is invoked.
    /// </summary>
    public class ConstructorParameterBinding
    {
        readonly ConstructorInfo _ci;
        readonly Func<object>[] _valueRetrievers;
        readonly bool _canInstantiate;

        // We really need to report all non-bindable parameters, howevers some refactoring
        // will be necessary before this is possible. Adding this now to ease the
        // pain of working with the preview builds.
        readonly ParameterInfo _firstNonBindableParameter;

        /// <summary>
        /// The constructor on the target type. The actual constructor used
        /// might differ, e.g. if using a dynamic proxy.
        /// </summary>
        public ConstructorInfo TargetConstructor { get { return _ci; } }

        /// <summary>
        /// True if the binding is valid.
        /// </summary>
        public bool CanInstantiate { get { return _canInstantiate; } }

        /// <summary>
        /// Construct a new ConstructorParameterBinding.
        /// </summary>
        /// <param name="ci">ConstructorInfo to bind.</param>
        /// <param name="availableParameters">Available parameters.</param>
        /// <param name="context">Context in which to construct instance.</param>
        public ConstructorParameterBinding(
            ConstructorInfo ci,
            IEnumerable<Parameter> availableParameters,
            IComponentContext context)
        {
            _canInstantiate = true;
            _ci = Enforce.ArgumentNotNull(ci, "ci");
            Enforce.ArgumentNotNull(availableParameters, "availableParameters");
            Enforce.ArgumentNotNull(context, "context");

            var parameters = ci.GetParameters();
            _valueRetrievers = new Func<object>[parameters.Length];

            for (int i = 0; i < parameters.Length; ++i)
            {
                var pi = parameters[i];
                bool foundValue = false;
                foreach (var param in availableParameters)
                {
                    Func<object> valueRetriever;
                    if (param.CanSupplyValue(pi, context, out valueRetriever))
                    {
                        _valueRetrievers[i] = valueRetriever;
                        foundValue = true;
                        break;
                    }
                }
                if (!foundValue)
                {
                    _canInstantiate = false;
                    _firstNonBindableParameter = pi;
                    break;
                }
            }
        }

        /// <summary>
        /// Invoke the constructor with the parameter bindings.
        /// </summary>
        /// <returns>The constructed instance.</returns>
        public object Instantiate()
        {
            if (!CanInstantiate)
                throw new InvalidOperationException();

            var values = new object[_valueRetrievers.Length];
            for (int i = 0; i < _valueRetrievers.Length; ++i)
                values[i] = _valueRetrievers[i].Invoke();

            return TargetConstructor.Invoke(values);
        }

        /// <summary>
        /// Describes the constructor parameter binding.
        /// </summary>
        public string Description
        {
            get
            {
                if (CanInstantiate)
                    return string.Format(ConstructorParameterBindingResources.BoundConstructor, _ci);
                
                return string.Format(ConstructorParameterBindingResources.NonBindableConstructor, _ci, _firstNonBindableParameter);
            }
        }

        ///<summary>Returns a System.String that represents the current System.Object.</summary>
        ///<returns>A System.String that represents the current System.Object.</returns>
        public override string ToString()
        {
            return Description;
        }
    }
}
