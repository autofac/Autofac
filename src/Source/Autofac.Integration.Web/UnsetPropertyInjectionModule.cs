using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Dependency injection module that will always inject any resolvable
    /// properties.
    /// </summary>
    public class UnsetPropertyInjectionModule : DependencyInjectionModule
    {
        IInjectionBehaviour _injectUnsetProperties = new UnsetPropertyInjection();

        /// <summary>
        /// Override to customise injection behaviour based on HTTP Handler type.
        /// </summary>
        /// <param name="handlerType">Type of the handler.</param>
        /// <returns>The injection behaviour.</returns>
        protected override IInjectionBehaviour GetInjectionBehaviourForHandlerType(Type handlerType)
        {
            if (handlerType == null)
                throw new ArgumentNullException("handlerType");

            return _injectUnsetProperties;
        }
    }
}
