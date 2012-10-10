using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Autofac.Core;
using Castle.DynamicProxy;

namespace Autofac.Extras.DynamicProxy2
{
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class InterceptAttribute : Attribute
    {
        public Service InterceptorService { get; private set; }

        public InterceptAttribute(Service interceptorService)
        {
            if (interceptorService == null)
                throw new ArgumentNullException("interceptorService");

            this.InterceptorService = interceptorService;
        }

        [SecuritySafeCritical]
        public InterceptAttribute(string interceptorServiceName)
            : this(new KeyedService(interceptorServiceName, typeof(IInterceptor)))
        {
        }

        public InterceptAttribute(Type interceptorServiceType)
            : this(new TypedService(interceptorServiceType))
        {
        }
    }
}
