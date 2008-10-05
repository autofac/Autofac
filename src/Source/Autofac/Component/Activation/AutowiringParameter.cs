using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Autofac.Component.Activation
{
    internal class AutowiringParameter : Parameter
    {
        public override bool CanSupplyValue(ParameterInfo pi, IContext context, out Func<object> valueProvider)
        {
            if (context.IsRegistered(pi.ParameterType))
            {
                valueProvider = () => MatchTypes(pi, context.Resolve(pi.ParameterType, Enumerable.Empty<Parameter>()));
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
