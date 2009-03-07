using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace AutofacContrib.DynamicProxy2
{
    public class ImplementationAttributeInterceptorProvider : IComponentInterceptorProvider
    {
        static readonly IEnumerable<Service> EmptyInterceptorServices = new Service[0];

        public IEnumerable<Service> GetInterceptorServices(IComponentDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException("descriptor");

            Type implType = descriptor.BestKnownImplementationType;
            if (implType.IsClass)
            {
                return implType
                    .GetCustomAttributes(typeof(InterceptAttribute), true)
                    .Cast<InterceptAttribute>()
                    .Select(att => att.InterceptorService)
                    .ToList();
            }
            else
            {
                return EmptyInterceptorServices;
            }
        }
    }
}
