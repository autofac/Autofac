using System.Collections.Generic;
using Autofac;

namespace AutofacContrib.DynamicProxy2
{
    public interface IComponentInterceptorProvider
    {
        IEnumerable<Service> GetInterceptorServices(IComponentDescriptor descriptor);
    }
}
