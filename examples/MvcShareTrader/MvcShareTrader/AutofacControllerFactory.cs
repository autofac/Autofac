using System;
using System.Web.Mvc;

namespace MvcShareTrader
{
    public class AutofacControllerFactory : IControllerFactory
    {
        #region IControllerFactory Members

        public IController CreateController(RequestContext context, Type controllerType)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (controllerType == null)
                throw new ArgumentNullException("controllerType");

            return (IController)AutofacHttpModule.RequestContainer.Resolve(controllerType);
        }

        #endregion
    }
}
