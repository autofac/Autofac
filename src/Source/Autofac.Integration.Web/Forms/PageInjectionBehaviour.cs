// Contributed by // This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Web.UI;

namespace Autofac.Integration.Web.Forms
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
        public void InjectDependencies(IComponentContext context, object target)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (target == null)
                throw new ArgumentNullException("target");

            var injector = GetInjector(context);

            DoInjection(injector, target);
        }

        /// <summary>
        /// Override to return a closure that injects properties into a target.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The injector.</returns>
        protected abstract Func<object, object> GetInjector(IComponentContext context);

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
                page.PreLoad += (s, e) => InjectUserControls(injector, page);
        }

        static void InjectUserControls(Func<object, object> injector, Control parent)
        {
            if (injector == null)
                throw new ArgumentNullException("injector");

            if (parent == null)
                throw new ArgumentNullException("parent");

            if (parent.Controls == null) 
                return;

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


