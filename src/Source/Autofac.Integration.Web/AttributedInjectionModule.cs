using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Injects dependencies into request handlers and pages that have been
    /// decorated with the [InjectProperties] or [InjectUnsetProperties]
    /// attributes.
    /// </summary>
    public class AttributedInjectionModule : DependencyInjectionModule
    {
        private IInjectionBehaviour _attributedInjection = new AttributedInjection();

        /// <summary>
        /// Override to customise injection behaviour based on HTTP Handler type.
        /// </summary>
        /// <param name="handlerType">Type of the handler.</param>
        /// <returns>The injection behaviour.</returns>
        protected override IInjectionBehaviour GetInjectionBehaviourForHandlerType(Type handlerType)
        {
            if (handlerType == null)
                throw new ArgumentNullException("handlerType");

            return _attributedInjection;
        }
    }
}
