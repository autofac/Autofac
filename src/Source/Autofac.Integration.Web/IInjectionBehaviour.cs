using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Provides dependency injection for a request handler.
    /// </summary>
    interface IInjectionBehaviour
    {
        /// <summary>
        /// Inject dependencies in the required fashion.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        void Inject(IContext context, object target);
    }
}
