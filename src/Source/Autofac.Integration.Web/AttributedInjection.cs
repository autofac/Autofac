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
    class AttributedInjection : PageInjectionBehaviour
    {
        /// <summary>
        /// Override to return a closure that injects properties into a target.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected override Func<object, object> GetInjector(IContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            return target =>
            {
                var targetType = target.GetType();
                if (targetType.GetCustomAttributes(typeof(InjectPropertiesAttribute), true).Length > 0)
                {
                    return context.InjectProperties(target);
                }
                else if (targetType.GetCustomAttributes(typeof(InjectUnsetPropertiesAttribute), true).Length > 0)
                {
                    return context.InjectUnsetProperties(target);
                }
                else
                {
                    return target;
                }
            };
        }
    }
}
