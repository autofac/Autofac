using System;
using System.Linq;
using Autofac;
using Autofac.Core;

namespace AutofacContrib.DynamicProxy2
{
    public class InterceptionModule : Module
    {
        readonly IComponentInterceptorProvider _provider;
        readonly IComponentInterceptorAttacher _attacher;

        public InterceptionModule(IComponentInterceptorProvider provider, IComponentInterceptorAttacher attacher)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (attacher == null)
                throw new ArgumentNullException("attacher");

            _provider = provider;
            _attacher = attacher;
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            base.AttachToComponentRegistration(componentRegistry, registration);

            var interceptorServices = _provider.GetInterceptorServices(registration);
            if (interceptorServices.Any())
                _attacher.AttachInterceptors(registration, interceptorServices);
        }
    }
}
