// This software is part of the Autofac IoC container
// Copyright © 2012 Autofac Contributors
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
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Defines a filter provider for filter attributes that performs property injection.
    /// </summary>
    public class AutofacFilterProvider : FilterAttributeFilterProvider
    {
        class FilterContext
        {
            public ActionDescriptor ActionDescriptor { get; set; }
            public ILifetimeScope LifetimeScope { get; set; }
            public Type ControllerType { get; set; }
            public List<Filter> Filters { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacFilterProvider"/> class.
        /// </summary>
        /// <remarks>
        /// The <c>false</c> constructor parameter passed to base here ensures that attribute instances are not cached.
        /// </remarks>
        public AutofacFilterProvider() : base(false)
        {
        }

        /// <summary>
        /// Aggregates the filters from all of the filter providers into one collection.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>
        /// The collection filters from all of the filter providers with properties injected.
        /// </returns>
        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor).ToList();
            var lifetimeScope = AutofacDependencyResolver.Current.RequestLifetimeScope;

            if (lifetimeScope != null)
            {
                foreach (var filter in filters)
                    lifetimeScope.InjectProperties(filter.Instance);

                var controllerType = controllerContext.Controller.GetType();

                var filterContext = new FilterContext
                {
                    ActionDescriptor = actionDescriptor,
                    LifetimeScope = lifetimeScope,
                    ControllerType = controllerType,
                    Filters = filters
                };

                ResolveControllerScopedFilters(filterContext);

                ResolveActionScopedFilters<ReflectedActionDescriptor>(filterContext, d => d.MethodInfo);
                ResolveActionScopedFilters<ReflectedAsyncActionDescriptor>(filterContext, d => d.AsyncMethodInfo);
                ResolveActionScopedFilters<TaskAsyncActionDescriptor>(filterContext, d => d.TaskMethodInfo);
            }

            return filters.ToArray();
        }

        static void ResolveControllerScopedFilters(FilterContext filterContext)
        {
            ResolveControllerScopedFilter<IActionFilter>(filterContext);
            ResolveControllerScopedFilter<IAuthorizationFilter>(filterContext);
            ResolveControllerScopedFilter<IExceptionFilter>(filterContext);
            ResolveControllerScopedFilter<IResultFilter>(filterContext);
        }

        static void ResolveControllerScopedFilter<TFilter>(FilterContext filterContext) 
            where TFilter : class
        {
            var actionFilters = filterContext.LifetimeScope.Resolve<IEnumerable<Lazy<TFilter, IFilterMetadata>>>();
            foreach (var actionFilter in actionFilters)
            {
                var metadata = actionFilter.Metadata;
                if (metadata.ControllerType.IsAssignableFrom(filterContext.ControllerType)
                    && metadata.FilterScope == FilterScope.Controller
                    && metadata.MethodInfo == null)
                {
                    var filter = new Filter(actionFilter.Value, FilterScope.Controller, metadata.Order);
                    filterContext.Filters.Add(filter);
                }
            }
        }

        static void ResolveActionScopedFilters<T>(FilterContext filterContext, Func<T, MethodInfo> methodSelector)
            where T : ActionDescriptor
        {
            var actionDescriptor = filterContext.ActionDescriptor as T;
            if (actionDescriptor == null) return;

            var methodInfo = methodSelector(actionDescriptor);

            ResolveActionScopedFilter<IActionFilter>(filterContext, methodInfo);
            ResolveActionScopedFilter<IAuthorizationFilter>(filterContext, methodInfo);
            ResolveActionScopedFilter<IExceptionFilter>(filterContext, methodInfo);
            ResolveActionScopedFilter<IResultFilter>(filterContext, methodInfo);
        }

        static void ResolveActionScopedFilter<TFilter>(FilterContext filterContext, MethodInfo methodInfo) 
            where TFilter : class
        {
            var actionFilters = filterContext.LifetimeScope.Resolve<IEnumerable<Lazy<TFilter, IFilterMetadata>>>();
            foreach (var actionFilter in actionFilters)
            {
                var metadata = actionFilter.Metadata;
                if (metadata.ControllerType.IsAssignableFrom(filterContext.ControllerType)
                    && metadata.FilterScope == FilterScope.Action
                    && metadata.MethodInfo.GetBaseDefinition() == methodInfo.GetBaseDefinition())
                {
                    var filter = new Filter(actionFilter.Value, FilterScope.Action, metadata.Order);
                    filterContext.Filters.Add(filter);
                }
            }
        }
    }
}
