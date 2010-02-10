using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Autofac.Core;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace AutofacContrib.DynamicProxy2
{
    public class TypedServiceInterceptorAttacher : IComponentInterceptorAttacher
    {
        readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public void AttachInterceptors(IComponentRegistration registration, IEnumerable<Service> interceptorServices)
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            if (interceptorServices == null)
                throw new ArgumentNullException("interceptorServices");

            var proxiedInterfaces = GetProxiedInterfaces(registration);

            var theInterface = proxiedInterfaces.First();
            var interfaces = proxiedInterfaces.Skip(1);

            registration.Activating += (s, e) =>
            {
                var interceptors = interceptorServices
                    .Select(svc => e.Context.Resolve(svc))
                    .Cast<IInterceptor>()
                    .ToArray();

                var interfacesWithIDisposable = interfaces;
                if (e.Instance is IDisposable)
                    interfacesWithIDisposable = interfaces.Concat(new[] { typeof(IDisposable) });

                e.Instance = _proxyGenerator.CreateInterfaceProxyWithTarget(
                    theInterface, interfacesWithIDisposable.ToArray(), e.Instance, interceptors);
            };
        }

        static IEnumerable<Type> GetProxiedInterfaces(IComponentRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            var proxiedInterfaces = registration
                .Services
                .OfType<IServiceWithType>()
                .Select(ts => ts.ServiceType);

            if (!proxiedInterfaces.Any())
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, TypedServiceInterceptorAttacherResources.ComponentDoesNotProvideInterceptibleServices, registration));

            if (proxiedInterfaces.Any(i => !i.IsInterface))
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, TypedServiceInterceptorAttacherResources.OnlyInterfacesSupported, registration));
            
            return proxiedInterfaces;
        }
    }
}
