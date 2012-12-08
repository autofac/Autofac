using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.Web;

namespace Remember.Web
{
    public class MvcContainerProvider : IContainerProvider
    {
        public void EndRequestLifetime()
        {
            // The AutofacDependencyResolver will handle ending the request lifetime.
            throw new NotSupportedException();
        }

        public ILifetimeScope ApplicationContainer
        {
            get
            {
                return AutofacDependencyResolver.Current.ApplicationContainer;
            }
        }

        public ILifetimeScope RequestLifetime
        {
            get
            {
                return AutofacDependencyResolver.Current.RequestLifetimeScope;
            }
        }
    }
}