using System;
using Autofac;
using Autofac.Core;
using Castle.Core.Interceptor;

namespace AutofacContrib.DynamicProxy2
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
            : this(new NamedService(interceptorServiceName, typeof(IInterceptor)))
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
