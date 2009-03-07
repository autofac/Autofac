using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace AutofacContrib.DynamicProxy2
{
    public class FlexibleInterceptorProvider : IComponentInterceptorProvider
    {
        IComponentInterceptorProvider
            _attributeProvider = new ImplementationAttributeInterceptorProvider(),
            _extendedPropertyProvider = new ExtendedPropertyInterceptorProvider();

        public IEnumerable<Service> GetInterceptorServices(IComponentDescriptor descriptor)
        {
            return _attributeProvider.GetInterceptorServices(descriptor)
                .Concat(_extendedPropertyProvider.GetInterceptorServices(descriptor))
                .Distinct();
        }
    }
}
