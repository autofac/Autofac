using System;
using System.Web.Mvc;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// An MS-MVC controller factory that returns controllers built by an
    /// Autofac IoC container scoped according to the current request.
    /// </summary>
    public class AutofacControllerFactory : IControllerFactory
    {
        #region IControllerFactory Members

        /// <summary>
        /// Creates the controller.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="controllerType">Type of the controller.</param>
        /// <returns></returns>
        public IController CreateController(RequestContext context, Type controllerType)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (controllerType == null)
                throw new ArgumentNullException("controllerType");

            return (IController)ObtainContainer().Resolve(controllerType);
        }

        #endregion

        protected virtual IContainer ObtainContainer()
        {
            return AutofacMvcIntegration.RequestContainer;
        }
    }
}
