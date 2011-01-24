// This software is part of the Autofac IoC container
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
using System.Collections.Generic;
using System.Web.Mvc;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Injects services from the container into the ASP.NET MVC invocation pipeline.
    /// This is a Async Controller Action Invoker which can be used for both async and non-async scenarios
    /// </summary>
    /// <remarks>
    /// <para>
    /// The following types, if registered in the container, will be added to the
    /// filters list:
    /// <list>
    /// <item><see cref="IActionFilter"/></item>
    /// <item><see cref="IAuthorizationFilter"/></item>
    /// <item><see cref="IExceptionFilter"/></item>
    /// <item><see cref="IResultFilter"/></item>
    /// </list>
    /// Any existing filters (i.e. added via attributes) will have properties
    /// injected.
    /// </para>
    /// <para>
    /// Action methods can include parameters that will be resolved from the
    /// container, along with regular parameters.
    /// </para>
    /// </remarks>
    public class ExtensibleActionInvoker : System.Web.Mvc.Async.AsyncControllerActionInvoker
    {
        readonly IComponentContext _context;
        readonly IEnumerable<IActionFilter> _actionFilters;
        readonly IEnumerable<IAuthorizationFilter> _authorizationFilters;
        readonly IEnumerable<IExceptionFilter> _exceptionFilters;
        readonly IEnumerable<IResultFilter> _resultFilters;
        readonly bool _injectActionMethodParameters;

        /// <summary>
        /// Create an instance of the action invoker.
        /// </summary>
        /// <param name="context">Context from which to inject dependencies.</param>
        /// <param name="actionFilters">The action filters.</param>
        /// <param name="authorizationFilters">The authorization filters.</param>
        /// <param name="exceptionFilters">The exception filters.</param>
        /// <param name="resultFilters">The result filters.</param>
        /// <param name="injectActionMethodParameters">If set to true, the action invoker will attempt to resolve
        /// the parameters of action methods via the container.</param>
        public ExtensibleActionInvoker(
            IComponentContext context,
            IEnumerable<IActionFilter> actionFilters,
            IEnumerable<IAuthorizationFilter> authorizationFilters,
            IEnumerable<IExceptionFilter> exceptionFilters,
            IEnumerable<IResultFilter> resultFilters,
            bool injectActionMethodParameters = false)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (actionFilters == null) throw new ArgumentNullException("actionFilters");
            if (authorizationFilters == null) throw new ArgumentNullException("authorizationFilters");
            if (exceptionFilters == null) throw new ArgumentNullException("exceptionFilters");
            if (resultFilters == null) throw new ArgumentNullException("resultFilters");
            _context = context;
            _actionFilters = actionFilters;
            _authorizationFilters = authorizationFilters;
            _exceptionFilters = exceptionFilters;
            _resultFilters = resultFilters;
            _injectActionMethodParameters = injectActionMethodParameters;
        }

        /// <summary>
        /// Gets the filters.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param><param name="actionDescriptor">The action descriptor.</param>
        /// <returns>
        /// The filter information object.
        /// </returns>
        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor);
            SetFilters(filters.ActionFilters, _actionFilters);
            SetFilters(filters.AuthorizationFilters, _authorizationFilters);
            SetFilters(filters.ExceptionFilters, _exceptionFilters);
            SetFilters(filters.ResultFilters, _resultFilters);
            return filters;
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param><param name="parameterDescriptor">The parameter descriptor.</param>
        /// <returns>
        /// The parameter value.
        /// </returns>
        protected override object GetParameterValue(ControllerContext controllerContext, ParameterDescriptor parameterDescriptor)
        {
            if (_injectActionMethodParameters)
                return _context.ResolveOptional(parameterDescriptor.ParameterType) ?? base.GetParameterValue(controllerContext, parameterDescriptor);

            return base.GetParameterValue(controllerContext, parameterDescriptor);
        }

        void SetFilters<T>(ICollection<T> existing, IEnumerable<T> additional)
        {
            foreach (var filter in existing)
                _context.InjectProperties(filter);

            foreach (var add in additional)
                existing.Add(add);
        }
    }
}