using System;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Injects any resolvable properties.
    /// </summary>
    class PropertyInjection : PageInjectionBehaviour
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

            return context.InjectProperties<object>;
        }
    }
}
