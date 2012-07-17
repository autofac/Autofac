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
using Autofac.Features.Metadata;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Defines a filter provider for filter attributes that performs property injection.
    /// </summary>
    public class AutofacFilterProvider : FilterAttributeFilterProvider
    {
        /// <summary>
        /// The metadata key used for the Order value of a filter.
        /// </summary>
        internal static readonly string FilterOrderKey = "FilterOrder";

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

                ResolveControllerScopedFilters(lifetimeScope, controllerType, filters);

                ResolveActionScopedFilters<ReflectedActionDescriptor>(actionDescriptor, lifetimeScope, controllerType, filters, d => d.MethodInfo);
                ResolveActionScopedFilters<ReflectedAsyncActionDescriptor>(actionDescriptor, lifetimeScope, controllerType, filters, d => d.AsyncMethodInfo);
                ResolveActionScopedFilters<TaskAsyncActionDescriptor>(actionDescriptor, lifetimeScope, controllerType, filters, d => d.TaskMethodInfo);
            }

            return filters.ToArray();
        }

        static void ResolveControllerScopedFilters(IComponentContext lifetimeScope, Type controllerType, ICollection<Filter> filters)
        {
            ResolveControllerScopedFilter<IActionFilter>(filters, lifetimeScope, controllerType);
            ResolveControllerScopedFilter<IAuthorizationFilter>(filters, lifetimeScope, controllerType);
            ResolveControllerScopedFilter<IExceptionFilter>(filters, lifetimeScope, controllerType);
            ResolveControllerScopedFilter<IResultFilter>(filters, lifetimeScope, controllerType);
        }

        static void ResolveControllerScopedFilter<TFilter>(ICollection<Filter> filters, IComponentContext lifetimeScope, Type controllerType) 
            where TFilter : class
        {
            var key = new FilterKey(controllerType, FilterScope.Controller, null);
            var actionFilters = lifetimeScope.ResolveOptionalKeyed<IEnumerable<Meta<TFilter>>>(key);
            if (actionFilters == null) return;

            foreach (var filter in actionFilters.Select(actionFilter => new Filter(
                actionFilter.Value, FilterScope.Controller, GetFilterOrder(actionFilter))))
            {
                filters.Add(filter);
            }
        }

        static void ResolveActionScopedFilters<T>(ActionDescriptor descriptor, IComponentContext lifetimeScope, Type controllerType, ICollection<Filter> filters, Func<T, MethodInfo> methodSelector)
            where T : ActionDescriptor
        {
            var actionDescriptor = descriptor as T;
            if (actionDescriptor == null) return;

            var methodInfo = methodSelector(actionDescriptor);

            ResolveActionScopedFilter<IActionFilter>(filters, lifetimeScope, methodInfo, controllerType);
            ResolveActionScopedFilter<IAuthorizationFilter>(filters, lifetimeScope, methodInfo, controllerType);
            ResolveActionScopedFilter<IExceptionFilter>(filters, lifetimeScope, methodInfo, controllerType);
            ResolveActionScopedFilter<IResultFilter>(filters, lifetimeScope, methodInfo, controllerType);
        }

        static void ResolveActionScopedFilter<TFilter>(ICollection<Filter> filters, IComponentContext lifetimeScope, MethodInfo methodInfo, Type controllerType) 
            where TFilter : class
        {
            var key = new FilterKey(controllerType, FilterScope.Action, methodInfo);
            var actionFilters = lifetimeScope.ResolveOptionalKeyed<IEnumerable<Meta<TFilter>>>(key);
            if (actionFilters == null) return;

            foreach (var filter in actionFilters.Select(actionFilter => new Filter(
                actionFilter.Value, FilterScope.Action, GetFilterOrder(actionFilter))))
            {
                filters.Add(filter);
            }
        }

        static int GetFilterOrder<TFilter>(Meta<TFilter> actionFilter) where TFilter : class
        {
            var order = Filter.DefaultOrder;
            if (actionFilter.Metadata.ContainsKey(FilterOrderKey))
            {
                order = (int) actionFilter.Metadata[FilterOrderKey];
            }
            return order;
        }
    }
}
