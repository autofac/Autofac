// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
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
using System.Linq;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Resolves a filter for the specified metadata for each controller request.
    /// </summary>
    class ExceptionFilterWrapper : ExceptionFilterAttribute, IAutofacExceptionFilter
    {
        readonly IFilterMetadata _filterMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionFilterWrapper"/> class.
        /// </summary>
        /// <param name="filterMetadata">The filter metadata.</param>
        public ExceptionFilterWrapper(IFilterMetadata filterMetadata)
        {
            if (filterMetadata == null) throw new ArgumentNullException("filterMetadata");

            _filterMetadata = filterMetadata;
        }

        /// <summary>
        /// Called when an exception is thrown.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var dependencyScope = actionExecutedContext.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Lazy<IAutofacExceptionFilter, IFilterMetadata>>>();

            foreach (var filter in filters.Where(FilterMatchesMetadata))
                filter.Value.OnException(actionExecutedContext);
        }

        bool FilterMatchesMetadata(Lazy<IAutofacExceptionFilter, IFilterMetadata> filter)
        {
            return filter.Metadata.ControllerType == _filterMetadata.ControllerType
                   && filter.Metadata.FilterScope == _filterMetadata.FilterScope
                   && filter.Metadata.MethodInfo == _filterMetadata.MethodInfo;
        }
    }
}