using System;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Injects resolvable properties that do not already have a value.
    /// </summary>
    class UnsetPropertyInjection: PageInjectionBehaviour
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

            return context.InjectUnsetProperties<object>;
        }
    }
}
