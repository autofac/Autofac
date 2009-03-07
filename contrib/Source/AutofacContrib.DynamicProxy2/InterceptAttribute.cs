using System;
using Autofac;

namespace AutofacContrib.DynamicProxy2
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InterceptAttribute : Attribute
    {
        Service _interceptorService;

        public InterceptAttribute(Service interceptorService)
        {
            if (interceptorService == null)
                throw new ArgumentNullException("interceptorService");

            _interceptorService = interceptorService;
        }

        public InterceptAttribute(string interceptorServiceName)
            : this(new NamedService(interceptorServiceName))
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
