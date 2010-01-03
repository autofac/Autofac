using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace AutofacContrib.DynamicProxy2
{
    public class ImplementationTypeInterceptorAttacher : IComponentInterceptorAttacher
    {
        ProxyGenerator _proxyGenerator = new ProxyGenerator();

        public void AttachInterceptors(IComponentRegistration registration, IEnumerable<Service> interceptorServices)
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            if (interceptorServices == null)
                throw new ArgumentNullException("interceptorServices");

            var reflectionActivator = registration.Activator as ReflectionActivator;
            if (reflectionActivator == null)
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
                    ImplementationTypeInterceptorAttacherResources.OnlyReflectionActivatorIsSupported,
                    registration,
                    registration.Activator));

            EnableDynamicInterceptionOnImplementationType(reflectionActivator);

            registration.Preparing += (sender, e) =>
            {
                e.Parameters = e.Parameters.Union(new Parameter[] {
                    TypedParameter.From(
                        interceptorServices.Select(s => e.Context.Resolve(s)).Cast<IInterceptor>().ToArray())
                }).ToArray();
            };
        }

        private void EnableDynamicInterceptionOnImplementationType(ReflectionActivator reflectionActivator)
        {
            if (reflectionActivator == null)
                throw new ArgumentNullException("reflectionActivator");

            reflectionActivator.ConstructorFinder = new ProxyConstructorFinder(
                reflectionActivator.ConstructorFinder,
                _proxyGenerator.ProxyBuilder.CreateClassProxy(reflectionActivator.LimitType, ProxyGenerationOptions.Default));
        }
    }
}
