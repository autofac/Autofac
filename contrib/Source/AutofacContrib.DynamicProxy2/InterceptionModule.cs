using System;
using System.Linq;
using Autofac;
using Autofac.Builder;

namespace AutofacContrib.DynamicProxy2
{
    public class InterceptionModule : Module
    {
        IComponentInterceptorProvider _provider;
        IComponentInterceptorAttacher _attacher;

        public InterceptionModule(IComponentInterceptorProvider provider, IComponentInterceptorAttacher attacher)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (attacher == null)
                throw new ArgumentNullException("attacher");

            _provider = provider;
            _attacher = attacher;
        }

        protected override void AttachToComponentRegistration(IContainer container, IComponentRegistration registration)
        {
            base.AttachToComponentRegistration(container, registration);

            var interceptorServices = _provider.GetInterceptorServices(registration.Descriptor);
            if (interceptorServices.Any())
                _attacher.AttachInterceptors(registration, interceptorServices);
        }
    }
}
