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
using System.Security;
using System.Web.Mvc;
using System.Web.Mvc.Async;
using System.Web.Mvc.Filters;
using Autofac.Features.Metadata;

namespace Autofac.Integration.Mvc
{
    /// <summary>
    /// Defines a filter provider for filter attributes that performs property injection.
    /// </summary>
    [SecurityCritical]
    public class AutofacFilterProvider : FilterAttributeFilterProvider
    {
        class FilterContext
        {
            public ActionDescriptor ActionDescriptor { get; set; }
            public ILifetimeScope LifetimeScope { get; set; }
            public Type ControllerType { get; set; }
            public List<Filter> Filters { get; set; }
        }

        internal static string ActionFilterMetadataKey = "AutofacMvcActionFilter";
        internal static string ActionFilterOverrideMetadataKey = "AutofacMvcActionFilterOverride";

        internal static string AuthorizationFilterMetadataKey = "AutofacMvcAuthorizationFilter";
        internal static string AuthorizationFilterOverrideMetadataKey = "AutofacMvcAuthorizationFilterOverride";

        internal static string AuthenticationFilterMetadataKey = "AutofacMvcAuthenticationFilter";
        internal static string AuthenticationFilterOverrideMetadataKey = "AutofacMvcAuthenticationFilterOverride";

        internal static string ExceptionFilterMetadataKey = "AutofacMvcExceptionFilter";
        internal static string ExceptionFilterOverrideMetadataKey = "AutofacMvcExceptionFilterOverride";

        internal static string ResultFilterMetadataKey = "AutofacMvcResultFilter";
        internal static string ResultFilterOverrideMetadataKey = "AutofacMvcResultFilterOverride";

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
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="controllerContext" /> is <see langword="null" />.
        /// </exception>
        [SecurityCritical]
        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
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

                ResolveControllerScopedFilterOverrides(filterContext);

                ResolveActionScopedFilterOverrides<ReflectedActionDescriptor>(filterContext, d => d.MethodInfo);
                ResolveActionScopedFilterOverrides<ReflectedAsyncActionDescriptor>(filterContext, d => d.AsyncMethodInfo);
                ResolveActionScopedFilterOverrides<TaskAsyncActionDescriptor>(filterContext, d => d.TaskMethodInfo);

                ResolveControllerScopedEmptyOverrideFilters(filterContext);

                ResolveActionScopedEmptyOverrideFilters<ReflectedActionDescriptor>(filterContext, d => d.MethodInfo);
                ResolveActionScopedEmptyOverrideFilters<ReflectedAsyncActionDescriptor>(filterContext, d => d.AsyncMethodInfo);
                ResolveActionScopedEmptyOverrideFilters<TaskAsyncActionDescriptor>(filterContext, d => d.TaskMethodInfo);
            }

            return filters.ToArray();
        }

        static void ResolveControllerScopedFilters(FilterContext filterContext)
        {
            ResolveControllerScopedFilter<IActionFilter>(filterContext, ActionFilterMetadataKey);
            ResolveControllerScopedFilter<IAuthenticationFilter>(filterContext, AuthenticationFilterMetadataKey);
            ResolveControllerScopedFilter<IAuthorizationFilter>(filterContext, AuthorizationFilterMetadataKey);
            ResolveControllerScopedFilter<IExceptionFilter>(filterContext, ExceptionFilterMetadataKey);
            ResolveControllerScopedFilter<IResultFilter>(filterContext, ResultFilterMetadataKey);
        }

        static void ResolveControllerScopedFilterOverrides(FilterContext filterContext)
        {
            ResolveControllerScopedFilter<IActionFilter>(filterContext, ActionFilterOverrideMetadataKey, filter => new ActionFilterOverride(filter));
            ResolveControllerScopedFilter<IAuthenticationFilter>(filterContext, AuthenticationFilterOverrideMetadataKey, filter => new AuthenticationFilterOverride(filter));
            ResolveControllerScopedFilter<IAuthorizationFilter>(filterContext, AuthorizationFilterOverrideMetadataKey, filter => new AuthorizationFilterOverride(filter));
            ResolveControllerScopedFilter<IExceptionFilter>(filterContext, ExceptionFilterOverrideMetadataKey, filter => new ExceptionFilterOverride(filter));
            ResolveControllerScopedFilter<IResultFilter>(filterContext, ResultFilterOverrideMetadataKey, filter => new ResultFilterOverride(filter));
        }

        static void ResolveControllerScopedFilter<TFilter>(FilterContext filterContext, string metadataKey, Func<TFilter, TFilter> wrapperFactory = null)
            where TFilter : class
        {
            var actionFilters = filterContext.LifetimeScope.Resolve<IEnumerable<Meta<Lazy<TFilter>>>>();

            foreach (var actionFilter in actionFilters.Where(a => a.Metadata.ContainsKey(metadataKey) && a.Metadata[metadataKey] is FilterMetadata))
            {
                var metadata = (FilterMetadata)actionFilter.Metadata[metadataKey];
                if (!FilterMatchesController(filterContext, metadata)) continue;

                var instance = actionFilter.Value.Value;

                if (wrapperFactory != null)
                    instance = wrapperFactory(instance);

                var filter = new Filter(instance, FilterScope.Controller, metadata.Order);
                filterContext.Filters.Add(filter);
            }
        }

        static void ResolveActionScopedFilters<T>(FilterContext filterContext, Func<T, MethodInfo> methodSelector)
            where T : ActionDescriptor
        {
            var actionDescriptor = filterContext.ActionDescriptor as T;
            if (actionDescriptor == null) return;

            var methodInfo = methodSelector(actionDescriptor);

            ResolveActionScopedFilter<IActionFilter>(filterContext, methodInfo, ActionFilterMetadataKey);
            ResolveActionScopedFilter<IAuthenticationFilter>(filterContext, methodInfo, AuthenticationFilterMetadataKey);
            ResolveActionScopedFilter<IAuthorizationFilter>(filterContext, methodInfo, AuthorizationFilterMetadataKey);
            ResolveActionScopedFilter<IExceptionFilter>(filterContext, methodInfo, ExceptionFilterMetadataKey);
            ResolveActionScopedFilter<IResultFilter>(filterContext, methodInfo, ResultFilterMetadataKey);
        }

        static void ResolveActionScopedFilterOverrides<T>(FilterContext filterContext, Func<T, MethodInfo> methodSelector)
            where T : ActionDescriptor
        {
            var actionDescriptor = filterContext.ActionDescriptor as T;
            if (actionDescriptor == null) return;

            var methodInfo = methodSelector(actionDescriptor);

            ResolveActionScopedFilter<IActionFilter>(filterContext, methodInfo, ActionFilterOverrideMetadataKey, filter => new ActionFilterOverride(filter));
            ResolveActionScopedFilter<IAuthenticationFilter>(filterContext, methodInfo, AuthenticationFilterOverrideMetadataKey, filter => new AuthenticationFilterOverride(filter));
            ResolveActionScopedFilter<IAuthorizationFilter>(filterContext, methodInfo, AuthorizationFilterOverrideMetadataKey, filter => new AuthorizationFilterOverride(filter));
            ResolveActionScopedFilter<IExceptionFilter>(filterContext, methodInfo, ExceptionFilterOverrideMetadataKey, filter => new ExceptionFilterOverride(filter));
            ResolveActionScopedFilter<IResultFilter>(filterContext, methodInfo, ResultFilterOverrideMetadataKey, filter => new ResultFilterOverride(filter));
        }

        static void ResolveActionScopedFilter<TFilter>(FilterContext filterContext, MethodInfo methodInfo, string metadataKey, Func<TFilter, TFilter> wrapperFactory = null)
            where TFilter : class
        {
            var actionFilters = filterContext.LifetimeScope.Resolve<IEnumerable<Meta<Lazy<TFilter>>>>();

            foreach (var actionFilter in actionFilters.Where(a => a.Metadata.ContainsKey(metadataKey) && a.Metadata[metadataKey] is FilterMetadata))
            {
                var metadata = (FilterMetadata)actionFilter.Metadata[metadataKey];
                if (!FilterMatchesAction(filterContext, methodInfo, metadata)) continue;

                var instance = actionFilter.Value.Value;

                if (wrapperFactory != null)
                    instance = wrapperFactory(instance);

                var filter = new Filter(instance, FilterScope.Action, metadata.Order);
                filterContext.Filters.Add(filter);
            }
        }

        static void ResolveControllerScopedEmptyOverrideFilters(FilterContext filterContext)
        {
            ResolveControllerScopedOverrideFilter(filterContext, ActionFilterOverrideMetadataKey);
            ResolveControllerScopedOverrideFilter(filterContext, AuthenticationFilterOverrideMetadataKey);
            ResolveControllerScopedOverrideFilter(filterContext, AuthorizationFilterOverrideMetadataKey);
            ResolveControllerScopedOverrideFilter(filterContext, ExceptionFilterOverrideMetadataKey);
            ResolveControllerScopedOverrideFilter(filterContext, ResultFilterOverrideMetadataKey);
        }

        static void ResolveControllerScopedOverrideFilter(FilterContext filterContext, string metadataKey)
        {
            var actionFilters = filterContext.LifetimeScope.Resolve<IEnumerable<Meta<IOverrideFilter>>>();

            foreach (var actionFilter in actionFilters.Where(a => a.Metadata.ContainsKey(metadataKey) && a.Metadata[metadataKey] is FilterMetadata))
            {
                var metadata = (FilterMetadata)actionFilter.Metadata[metadataKey];
                if (!FilterMatchesController(filterContext, metadata)) continue;

                var filter = new Filter(actionFilter.Value, FilterScope.Controller, metadata.Order);
                filterContext.Filters.Add(filter);
            }
        }

        static void ResolveActionScopedEmptyOverrideFilters<T>(FilterContext filterContext, Func<T, MethodInfo> methodSelector)
            where T : ActionDescriptor
        {
            var actionDescriptor = filterContext.ActionDescriptor as T;
            if (actionDescriptor == null) return;

            var methodInfo = methodSelector(actionDescriptor);

            ResolveActionScopedOverrideFilter(filterContext, methodInfo, ActionFilterOverrideMetadataKey);
            ResolveActionScopedOverrideFilter(filterContext, methodInfo, AuthenticationFilterOverrideMetadataKey);
            ResolveActionScopedOverrideFilter(filterContext, methodInfo, AuthorizationFilterOverrideMetadataKey);
            ResolveActionScopedOverrideFilter(filterContext, methodInfo, ExceptionFilterOverrideMetadataKey);
            ResolveActionScopedOverrideFilter(filterContext, methodInfo, ResultFilterOverrideMetadataKey);
        }

        static void ResolveActionScopedOverrideFilter(FilterContext filterContext, MethodInfo methodInfo, string metadataKey)
        {
            var actionFilters = filterContext.LifetimeScope.Resolve<IEnumerable<Meta<IOverrideFilter>>>();

            foreach (var actionFilter in actionFilters.Where(a => a.Metadata.ContainsKey(metadataKey) && a.Metadata[metadataKey] is FilterMetadata))
            {
                var metadata = (FilterMetadata)actionFilter.Metadata[metadataKey];
                if (!FilterMatchesAction(filterContext, methodInfo, metadata)) continue;

                var filter = new Filter(actionFilter.Value, FilterScope.Action, metadata.Order);
                filterContext.Filters.Add(filter);
            }
        }

        static bool FilterMatchesController(FilterContext filterContext, FilterMetadata metadata)
        {
            return metadata.ControllerType != null
                   && metadata.ControllerType.IsAssignableFrom(filterContext.ControllerType)
                   && metadata.FilterScope == FilterScope.Controller
                   && metadata.MethodInfo == null;
        }

        static bool FilterMatchesAction(FilterContext filterContext, MethodInfo methodInfo, FilterMetadata metadata)
        {
            return metadata.ControllerType != null
                   && metadata.ControllerType.IsAssignableFrom(filterContext.ControllerType)
                   && metadata.FilterScope == FilterScope.Action
                   && metadata.MethodInfo.GetBaseDefinition() == methodInfo.GetBaseDefinition();
        }
    }
}
