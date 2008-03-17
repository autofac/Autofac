// Contributed by Nicholas Blumhardt 2008-01-28
// Copyright (c) 2008 Autofac Contributors
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
        /// <returns>The injector.</returns>
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
