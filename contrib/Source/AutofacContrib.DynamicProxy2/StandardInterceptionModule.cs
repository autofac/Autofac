using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutofacContrib.DynamicProxy2
{
    /// <summary>
    /// Follows the conventions typical in other containers - proxying by subclass only, interceptors determined
    /// using attribute.
    /// </summary>
    public class StandardInterceptionModule : InterceptionModule
    {
        public StandardInterceptionModule()
            : base(new ImplementationAttributeInterceptorProvider(), new ImplementationTypeInterceptorAttacher())
        {
        }
    }
}
