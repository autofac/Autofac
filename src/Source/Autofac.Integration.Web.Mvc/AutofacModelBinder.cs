using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Reflection;
using Autofac.Builder;
using Autofac.Features.Scanning;
using Autofac.Core;

namespace Autofac.Integration.Web.Mvc
{
    /// <summary>
    /// A wrapper ModelBinder that lazily resolves the inner model binder type
    /// from the request lifetime when BindModel is called.
    /// </summary>
    public class AutofacModelBinder : IModelBinder
    {

        private readonly Type _innerBinderType;

        public AutofacModelBinder(Type innerBinderType)
        {
            if (innerBinderType == null) throw new ArgumentNullException("innerBinderType");
            _innerBinderType = innerBinderType;
        }


        IContainerProvider GetContainerProvider(ControllerContext cc)
        {

            //TODO: move this string to Resources
            var webapp = cc.HttpContext.ApplicationInstance as IContainerProviderAccessor;
            if (webapp == null)
                throw new ArgumentException("HttpApplication doesn't implment IContainerProviderAccessor");

            return webapp.ContainerProvider;
        }

        /// <summary>
        /// Will resolve the innerBinderType from the Request Lifetime (or if it's not registered, uses
        /// Activator instead) then uses the instance to Bind the model
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            IModelBinder binder = null;

            var provider = GetContainerProvider(controllerContext);
            ILifetimeScope lifetimeScope = provider.RequestLifetime;
            if (lifetimeScope.IsRegistered(_innerBinderType))
            {
                binder = ((IModelBinder)lifetimeScope.Resolve(_innerBinderType));
            }
            else
            {
                binder = ((IModelBinder)Activator.CreateInstance(_innerBinderType));
            }

            return binder.BindModel(controllerContext, bindingContext);
        }
    }
}
