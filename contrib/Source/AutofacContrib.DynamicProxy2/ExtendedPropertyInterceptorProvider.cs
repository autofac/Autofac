using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Core;

namespace AutofacContrib.DynamicProxy2
{
    public class ExtendedPropertyInterceptorProvider : IComponentInterceptorProvider
    {
        public const string InterceptorsPropertyName = "AutofacContrib.DynamicProxy2.ExtendedPropertyInterceptorProvider.InterceptorsPropertyName";

        static readonly IEnumerable<Service> EmptyServices = new Service[0];

        public IEnumerable<Service>  GetInterceptorServices(IComponentRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            object services;
            if (registration.ExtendedProperties.TryGetValue(InterceptorsPropertyName, out services))
                return (IEnumerable<Service>)services;
            else
                return EmptyServices;
        }
    }
}
