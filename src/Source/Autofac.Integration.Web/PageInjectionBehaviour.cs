using System;
using System.Web.UI;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Assists with the construction of page injectors.
    /// </summary>
    abstract class PageInjectionBehaviour : IInjectionBehaviour
    {
        /// <summary>
        /// Inject dependencies in the required fashion.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        public void InjectDependencies(IContext context, object target)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (target == null)
                throw new ArgumentNullException("target");

            Func<object, object> injector = GetInjector(context);

            DoInjection(injector, target);
        }

        /// <summary>
        /// Override to return a closure that injects properties into a target.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected abstract Func<object, object> GetInjector(IContext context);

        /// <summary>
        /// Does the injection using a supplied injection function.
        /// </summary>
        /// <param name="injector">The injector.</param>
        /// <param name="target">The target.</param>
        void DoInjection(Func<object, object> injector, object target)
        {
            if (injector == null)
                throw new ArgumentNullException("injector");

            if (target == null)
                throw new ArgumentNullException("target");

            injector(target);

            var page = target as Page;
            if (page != null)
                page.InitComplete += (s, e) => InjectUserControls(injector, page);
        }

        void InjectUserControls(Func<object, object> injector, Control parent)
        {
            if (injector == null)
                throw new ArgumentNullException("injector");

            if (parent == null)
                throw new ArgumentNullException("parent");

            if (parent.Controls != null)
            {
                foreach (Control control in parent.Controls)
                {
                    var uc = control as UserControl;
                    if (uc != null)
                        injector(uc);
                    InjectUserControls(injector, control);
                }
            }
        }
    }
}
