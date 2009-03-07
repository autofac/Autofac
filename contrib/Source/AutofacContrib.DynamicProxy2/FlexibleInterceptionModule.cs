using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutofacContrib.DynamicProxy2
{
    public class FlexibleInterceptionModule : InterceptionModule
    {
        public FlexibleInterceptionModule()
            : base(new FlexibleInterceptorProvider(), new FlexibleInterceptorAttacher())
        {
        }
    }
}
