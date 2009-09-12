// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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

using System.Collections;
using System.Web.Mvc;

namespace Autofac.Integration.Web.Mvc
{
    /// <summary>
    /// Applies property injection to filters attached to the controller.
    /// </summary>
	public class FilterInjectionActionInvoker : ControllerActionInvoker
	{
		private IComponentContext _context;

        /// <summary>
        /// If true, only unset properties on filters will be injected.
        /// Defaults to false.
        /// </summary>
        public bool UnsetPropertiesOnly { get; set; }

        /// <summary>
        /// Create an instance of the action invoker.
        /// </summary>
        /// <param name="context">Context from which to inject dependencies.</param>
		public FilterInjectionActionInvoker(IComponentContext context)
		{
			_context = context;
		}

		protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor) 
		{
			var filters = base.GetFilters(controllerContext, actionDescriptor);
			InjectFilters(filters.ActionFilters);
			InjectFilters(filters.AuthorizationFilters);
			InjectFilters(filters.ExceptionFilters);
			InjectFilters(filters.ResultFilters);
			return filters;
		}

		void InjectFilters(IEnumerable filters)
		{
			foreach(var filter in filters)
			{
                if (UnsetPropertiesOnly)
                    _context.InjectUnsetProperties(filter);
                else
                    _context.InjectProperties(filter);
			}
		}
	}
}