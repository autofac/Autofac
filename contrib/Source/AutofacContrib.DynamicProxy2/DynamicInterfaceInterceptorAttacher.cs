using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace AutofacContrib.DynamicProxy2
{
    public class DynamicInterfaceInterceptorAttacher : IComponentInterceptorAttacher
    {
        ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public void AttachInterceptors(IComponentRegistration registration, IEnumerable<Service> interceptorServices)
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            if (interceptorServices == null)
                throw new ArgumentNullException("interceptorServices");

            registration.Activating += (sender, e) =>
            {
                var proxiedInterfaces = e.Instance.GetType().GetInterfaces();

                if (!proxiedInterfaces.Any())
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, DynamicInterfaceInterceptorAttacherResources.ComponentDoesNotProvideInterceptibleServices, registration));

                var theInterface = proxiedInterfaces.First();
                var interfaces = proxiedInterfaces.Skip(1).ToArray();

                var interceptors = interceptorServices.Select(s => e.Context.Resolve(s)).Cast<IInterceptor>().ToArray();

                e.Instance = _proxyGenerator.CreateInterfaceProxyWithTarget(theInterface, interfaces, e.Instance, interceptors);
            };
        }
    }
}
