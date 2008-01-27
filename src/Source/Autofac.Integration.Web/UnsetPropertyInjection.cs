using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Injects resolvable properties that do not already have a value.
    /// </summary>
    class UnsetPropertyInjection : IInjectionBehaviour
    {
        /// <summary>
        /// Inject dependencies in the required fashion.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        public void InjectDependencies(IContext context, object target)
        {
            context.InjectUnsetProperties(target);
        }
    }
}
