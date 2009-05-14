using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Autofac
{
    /// <summary>
    /// Base class for parameters that provide a constant value.
    /// </summary>
    public abstract class ConstantParameter : Parameter
    {
        Predicate<ParameterInfo> _predicate;

        /// <summary>
        /// The value of the parameter.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Create a constant parameter that will apply to parameters matching
        /// the supplied predicate.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="predicate"></param>
        protected ConstantParameter(object value, Predicate<ParameterInfo> predicate)
        {
            Value = value;
            _predicate = Enforce.ArgumentNotNull(predicate, "predicate");
        }

        /// <summary>
        /// Returns true if the parameter is able to provide a value to a particular site.
        /// </summary>
        /// <param name="pi">Constructor, method, or property-mutator parameter.</param>
        /// <param name="context">The component context in which the value is being provided.</param>
        /// <param name="valueProvider">If the result is true, the valueProvider parameter will
        /// be set to a function that will lazily retrieve the parameter value. If the result is false,
        /// will be set to null.</param>
        /// <returns>True if a value can be supplied; otherwise, false.</returns>
        public override bool CanSupplyValue(ParameterInfo pi, IContext context, out Func<object> valueProvider)
        {
            Enforce.ArgumentNotNull(pi, "pi");
            Enforce.ArgumentNotNull(context, "context");

            if (_predicate(pi))
            {
                valueProvider = () => MatchTypes(pi, Value);
                return true;
            }
            else
            {
                valueProvider = null;
                return false;
            }
        }
    }
}
