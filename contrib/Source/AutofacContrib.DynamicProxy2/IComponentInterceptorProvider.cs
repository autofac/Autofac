using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace AutofacContrib.DynamicProxy2
{
    public interface IComponentInterceptorProvider
    {
        IEnumerable<Service> GetInterceptorServices(IComponentRegistration registration);
    }
}
