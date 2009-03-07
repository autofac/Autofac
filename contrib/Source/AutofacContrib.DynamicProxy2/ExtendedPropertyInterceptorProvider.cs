using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace AutofacContrib.DynamicProxy2
{
    public class ExtendedPropertyInterceptorProvider : IComponentInterceptorProvider
    {
        public const string InterceptorsPropertyName = "AutofacContrib.DynamicProxy2.ExtendedPropertyInterceptorProvider.InterceptorsPropertyName";

        static readonly IEnumerable<Service> EmptyServices = new Service[0];

        public IEnumerable<Service>  GetInterceptorServices(IComponentDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException("descriptor");

            object services;
            if (descriptor.ExtendedProperties.TryGetValue(InterceptorsPropertyName, out services))
                return (IEnumerable<Service>)services;
            else
                return EmptyServices;
        }
    }
}
