using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Component.Tagged;
using Autofac.Component;
using Autofac.Component.Activation;

namespace AutofacContrib.DynamicProxy2
{
    public class FlexibleInterceptorAttacher : IComponentInterceptorAttacher
    {
        IComponentInterceptorAttacher _implementationTypeAttacher = new ImplementationTypeInterceptorAttacher(),
            _typedServiceAttacher = new TypedServiceInterceptorAttacher(),
            _dynamicInterfaceAttacher = new DynamicInterfaceInterceptorAttacher();

        public void AttachInterceptors(IComponentRegistration registration, IEnumerable<Service> interceptorServices)
        {
            if (registration == null)
                throw new ArgumentNullException("registration");

            if (interceptorServices == null)
                throw new ArgumentNullException("interceptorServices");

            var implementingRegistration = registration;
            while (implementingRegistration is IRegistrationDecorator)
                implementingRegistration = ((IRegistrationDecorator)implementingRegistration).InnerRegistration;

            if (implementingRegistration is Registration)
            {
                var activator = ((Registration)registration).Activator;
                if (activator is ReflectionActivator)
                {
                    _implementationTypeAttacher.AttachInterceptors(registration, interceptorServices);
                    return;
                }
            }

            if (implementingRegistration.Descriptor.Services.OfType<TypedServiceInterceptorAttacher>().Any())
            {
                _typedServiceAttacher.AttachInterceptors(registration, interceptorServices);
                return;
            }

            _dynamicInterfaceAttacher.AttachInterceptors(registration, interceptorServices);
        }
    }
}
