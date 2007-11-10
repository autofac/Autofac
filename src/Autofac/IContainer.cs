using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac
{
    interface IContainer
    {
        bool TryGetRegistration(
            string key, 
            out IComponentRegistration registration, 
            out IDisposer disposer,
            out IContext context);
    }
}
