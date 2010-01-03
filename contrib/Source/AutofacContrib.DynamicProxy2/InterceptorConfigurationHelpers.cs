using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;

namespace AutofacContrib.DynamicProxy2
{
    public static class InterceptorConfigurationHelpers
    {
        public static T InterceptedBy<T>(this T registrar, params Service[] interceptorServices)
            where T : IRegistrar<T>
        {
            if (registrar == null)
                throw new ArgumentNullException("registrar");

            if (interceptorServices == null || interceptorServices.Any(s => s == null))
                throw new ArgumentNullException("interceptorServices");

            var oldRegistrationCreator = registrar.RegistrationCreator;
            registrar.RegistrationCreator = (descriptor, activator, scope, ownership) =>
                oldRegistrationCreator(AddInterceptors(descriptor, interceptorServices), activator, scope, ownership);

            return registrar;
        }

        public static T InterceptedBy<T>(this T registrar, params string[] interceptorServiceNames)
            where T : IRegistrar<T>
        {
            if (interceptorServiceNames == null || interceptorServiceNames.Any(n => n == null))
                throw new ArgumentNullException("interceptorServiceNames");

            return InterceptedBy(registrar, interceptorServiceNames.Select(n => new NamedService(n)).ToArray());
        }

        public static T InterceptedBy<T>(this T registrar, params Type[] interceptorServiceTypes)
            where T : IRegistrar<T>
        {
            if (interceptorServiceTypes == null || interceptorServiceTypes.Any(t => t == null))
                throw new ArgumentNullException("interceptorServiceTypes");

            return InterceptedBy(registrar, interceptorServiceTypes.Select(t => new TypedService(t)).ToArray());
        }

        static IComponentDescriptor AddInterceptors(IComponentDescriptor descriptor, IEnumerable<Service> interceptorServices)
        {
            if (descriptor == null)
                throw new ArgumentNullException("descriptor");

            if (interceptorServices == null)
                throw new ArgumentNullException("interceptorServices");

            object existing;
            if (descriptor.ExtendedProperties.TryGetValue(ExtendedPropertyInterceptorProvider.InterceptorsPropertyName, out existing))
                descriptor.ExtendedProperties[ExtendedPropertyInterceptorProvider.InterceptorsPropertyName] =
                    ((IEnumerable<Service>)existing).Concat(interceptorServices).Distinct();
            else
                descriptor.ExtendedProperties.Add(ExtendedPropertyInterceptorProvider.InterceptorsPropertyName, interceptorServices);

            return descriptor;
        }
    }
}
