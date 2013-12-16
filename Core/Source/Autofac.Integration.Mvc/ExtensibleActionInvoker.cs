// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
using System.Security;
using System.Web.Mvc;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Injects services from the container into the ASP.NET MVC invocation pipeline.
    /// This is a Async Controller Action Invoker which can be used for both async and non-async scenarios
    /// </summary>
    /// <remarks>
    /// <para>
    /// Action methods can include parameters that will be resolved from the
    /// container, along with regular parameters.
    /// </para>
    /// </remarks>
    [SecurityCritical]
    public class ExtensibleActionInvoker : System.Web.Mvc.Async.AsyncControllerActionInvoker
    {
        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param><param name="parameterDescriptor">The parameter descriptor.</param>
        /// <returns>
        /// The parameter value.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="parameterDescriptor" /> is <see langword="null" />.
        /// </exception>
        [SecurityCritical]
        protected override object GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor)
        {
            if (parameterDescriptor == null)
            {
                throw new ArgumentNullException("parameterDescriptor");
            }

            // Issue #430
            // Model binding to collections (specifically IEnumerable<HttpPostedFileBase>
            // as used in multiple file upload scenarios) breaks if you try to
            // resolve before allowing default model binding to give it a shot.
            // You also can't send in an object that needs to be model bound if
            // it's registered in the container because the container will ignore
            // the POSTed in values.
            //
            // Issue #368
            // The original solution to issue #368 was to fall back to default
            // model binding if the ExtensibleActionInvoker was unable to resolve
            // a parameter AND if parameter injection was enabled. You can no longer
            // disable parameter injection, and it turns out for issue #430 that
            // we need to try model binding first. Unfortunately there's no way
            // to determine if default model binding will fail, so we give it
            // a shot and handle what we can.
            object value = null;
            try
            {
                value = base.GetParameterValue(controllerContext, parameterDescriptor);
            }
            catch (MissingMethodException)
            {
                // Don't do anything - this means the default model binder couldn't
                // activate a new instance (like if it's an interface) or figure
                // out some other way to model bind it.
            }

            if (value == null)
            {
                // We got nothing from the default model binding, so try to
                // resolve it.
                var context = AutofacDependencyResolver.Current.RequestLifetimeScope;
                value = context.ResolveOptional(parameterDescriptor.ParameterType);
            }

            return value;
        }
    }
}