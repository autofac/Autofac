using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Features.GeneratedFactories;

namespace Autofac.Util.Portable
{
    internal static class PortableExtensions
    {
        internal static ParameterMapping ResolveParameterMapping(this ParameterMapping configuredParameterMapping, Type delegateType)
        {
            if(configuredParameterMapping == ParameterMapping.Adaptive)
                return delegateType.Name.StartsWith("Func`") ? ParameterMapping.ByType : ParameterMapping.ByName;
            return configuredParameterMapping;
        }

        internal static IEnumerable<Parameter> GetParameterCollection<TDelegate>(this ParameterMapping mapping, params object[] param)
        {
            IEnumerable<Parameter> parameterCollection;

            switch(mapping)
            {
                case ParameterMapping.ByType:
                    parameterCollection = param.Select(x => (Parameter)new TypedParameter(x.GetType(), x));
                    break;
                case ParameterMapping.ByName:
                    {
                        var parameterInfo = typeof(TDelegate).GetMethods().First().GetParameters();
                        parameterCollection = new List<Parameter>();
                        for(var i = 0; i < param.Length; i++)
                        {
                            ((IList<Parameter>)parameterCollection).Add(new NamedParameter(parameterInfo[i].Name, param[i]));
                        }
                    }
                    break;
                case ParameterMapping.ByPosition:
                    parameterCollection = new List<Parameter>();
                    for(var i = 0; i < param.Length; i++)
                    {
                        ((IList<Parameter>)parameterCollection).Add(new PositionalParameter(i, param[i]));
                    }
                    break;
                default:
                    throw new NotSupportedException("Parameter mapping not supported");
            }

            return parameterCollection;
        }
    }
}
