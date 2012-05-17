using System;
using Autofac;
using Autofac.Core;
using Castle.DynamicProxy;

namespace Autofac.Extras.DynamicProxy2
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class InterceptAttribute : Attribute
    {
        readonly Service _interceptorService;

        public InterceptAttribute(Service interceptorService)
        {
            if (interceptorService == null)
                throw new ArgumentNullException("interceptorService");

            _interceptorService = interceptorService;
        }

        public InterceptAttribute(string interceptorServiceName)
            : this(new KeyedService(interceptorServiceName, typeof(IInterceptor)))
        {
        }

        public InterceptAttribute(Type interceptorServiceType)
            : this(new TypedService(interceptorServiceType))
        {
        }

        public Service InterceptorService
        {
            get
            {
                return _interceptorService;
            }
        }
    }
}
