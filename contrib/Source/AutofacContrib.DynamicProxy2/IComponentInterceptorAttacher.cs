using System.Collections.Generic;
using Autofac;

namespace AutofacContrib.DynamicProxy2
{
    public interface IComponentInterceptorAttacher
    {
        void AttachInterceptors(IComponentRegistration registration, IEnumerable<Service> interceptors);
    }
}
