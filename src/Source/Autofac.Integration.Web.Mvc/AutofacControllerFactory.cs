using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Autofac.Integration.Web.Mvc
{
    /// <summary>
    /// An MS-MVC controller factory that returns controllers built by an
    /// Autofac IoC container scoped according to the current request.
    /// </summary>
    public class AutofacControllerFactory : IControllerFactory
    {
        IContainerProvider _containerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacControllerFactory"/> class.
        /// </summary>
        /// <param name="containerProvider">The container provider.</param>
        public AutofacControllerFactory(IContainerProvider containerProvider)
        {
            if (containerProvider == null)
                throw new ArgumentNullException("containerProvider");

            _containerProvider = containerProvider;
        }

        /// <summary>
        /// Creates the controller.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns></returns>
        public virtual IController CreateController(RequestContext context, string controllerName)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (controllerName == null)
                throw new ArgumentNullException("controllerName");

            return _containerProvider.RequestContainer.Resolve<IController>(controllerName);
        }

        /// <summary>
        /// Disposes the controller. Unecessary in an Autofac-managed application.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public virtual void DisposeController(IController controller)
        {
        }
    }
}
