using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Autofac.Component;
using Autofac.Component.Activation;
using Autofac.Component.Tagged;
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

            var reflectionActivator = GetActivator(registration);

            EnableDynamicInterceptionOnImplementationType(reflectionActivator);

            registration.Preparing += (sender, e) =>
            {
                e.Parameters = e.Parameters.Union(new Parameter[] {
                    new NamedParameter(
                        ProxyConstructorInvoker.InterceptorsParameterName,
                        interceptorServices.Select(s => e.Context.Resolve(s)).Cast<IInterceptor>().ToArray())
                }).ToArray();
            };
        }

        private void EnableDynamicInterceptionOnImplementationType(ReflectionActivator reflectionActivator)
        {
            if (reflectionActivator == null)
                throw new ArgumentNullException("reflectionActivator");

            reflectionActivator.ConstructorInvoker = new ProxyConstructorInvoker(
                _proxyGenerator.ProxyBuilder.CreateClassProxy(reflectionActivator.ImplementationType, ProxyGenerationOptions.Default));
        }

        ReflectionActivator GetActivator(IComponentRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            var implementingRegistration = registration;
            while (implementingRegistration is IRegistrationDecorator) // e.g. a tagged registration
                implementingRegistration = ((IRegistrationDecorator)implementingRegistration).InnerRegistration;

            if (!(implementingRegistration is Registration))
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, ImplementationTypeInterceptorAttacherResources.OnlyStandardRegistrationSupported, registration));

            var activator = ((Registration)registration).Activator;
            if (!(activator is ReflectionActivator))
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, ImplementationTypeInterceptorAttacherResources.OnlyReflectionActivatorIsSupported, registration, activator));

            var reflectionActivator = (ReflectionActivator)activator;
            return reflectionActivator;
        }
    }
}
