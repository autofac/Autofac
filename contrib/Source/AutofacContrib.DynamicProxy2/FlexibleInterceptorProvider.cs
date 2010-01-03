using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Core;

namespace AutofacContrib.DynamicProxy2
{
    public class FlexibleInterceptorProvider : IComponentInterceptorProvider
    {
        IComponentInterceptorProvider
            _attributeProvider = new ImplementationAttributeInterceptorProvider(),
            _extendedPropertyProvider = new ExtendedPropertyInterceptorProvider();

        public IEnumerable<Service> GetInterceptorServices(IComponentRegistration registration)
        {
            return _attributeProvider.GetInterceptorServices(registration)
                .Concat(_extendedPropertyProvider.GetInterceptorServices(registration))
                .Distinct();
        }
    }
}
