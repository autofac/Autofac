using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;

namespace AutofacContrib.DynamicProxy2
{
    public class ImplementationAttributeInterceptorProvider : IComponentInterceptorProvider
    {
        static readonly IEnumerable<Service> EmptyInterceptorServices = new Service[0];

        public IEnumerable<Service> GetInterceptorServices(IComponentRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            Type implType = registration.Activator.LimitType;
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
