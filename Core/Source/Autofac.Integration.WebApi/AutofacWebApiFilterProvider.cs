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
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Linq;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// A filter provider for performing property injection on filter attributes.
    /// </summary>
    [SecurityCritical]
    public class AutofacWebApiFilterProvider : IFilterProvider
    {
        class FilterContext
        {
            public ILifetimeScope LifetimeScope { get; set; }
            public Type ControllerType { get; set; }
            public List<FilterInfo> Filters { get; set; }
            public Dictionary<string, List<FilterMetadata>> AddedFilters { get; set; }
        }

        readonly ILifetimeScope _rootLifetimeScope;
        readonly ActionDescriptorFilterProvider _filterProvider = new ActionDescriptorFilterProvider();

        internal static string ActionFilterMetadataKey = "AutofacWebApiActionFilter";

        internal static string AuthorizationFilterMetadataKey = "AutofacWebApiAuthorizationFilter";

        internal static string AuthenticationFilterMetadataKey = "AutofacWebApiAuthenticationFilter";

        internal static string ExceptionFilterMetadataKey = "AutofacWebApiExceptionFilter";

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacWebApiFilterProvider"/> class.
        /// </summary>
        public AutofacWebApiFilterProvider(ILifetimeScope lifetimeScope)
        {
            _rootLifetimeScope = lifetimeScope;
        }

        /// <summary>
        /// Returns the collection of filters associated with <paramref name="actionDescriptor"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>A collection of filters with instances property injected.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="configuration" /> is <see langword="null" />.
        /// </exception>
        [SecurityCritical]
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            var filters = _filterProvider.GetFilters(configuration, actionDescriptor).ToList();

            foreach (var filterInfo in filters)
                _rootLifetimeScope.InjectProperties(filterInfo.Instance);

            var descriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (descriptor == null) return filters;

            // Use a fake scope to resolve the metadata for the filter.
            var rootLifetimeScope = configuration.DependencyResolver.GetRootLifetimeScope();
            using (var lifetimeScope = rootLifetimeScope.BeginLifetimeScope(AutofacWebApiDependencyResolver.ApiRequestTag))
            {
                var filterContext = new FilterContext
                {
                    LifetimeScope = lifetimeScope,
                    ControllerType = actionDescriptor.ControllerDescriptor.ControllerType,
                    Filters = filters,
                    AddedFilters = new Dictionary<string, List<FilterMetadata>>
                    {
                        {ActionFilterMetadataKey, new List<FilterMetadata>()},
                        {AuthenticationFilterMetadataKey, new List<FilterMetadata>()},
                        {AuthorizationFilterMetadataKey, new List<FilterMetadata>()},
                        {ExceptionFilterMetadataKey, new List<FilterMetadata>()}
                    }
                };

                ResolveControllerScopedFilter<IAutofacActionFilter, ActionFilterWrapper>(
                    filterContext, m => new ActionFilterWrapper(m), ActionFilterMetadataKey);
                ResolveControllerScopedFilter<IAutofacAuthenticationFilter, AuthenticationFilterWrapper>(
                    filterContext, m => new AuthenticationFilterWrapper(m), AuthenticationFilterMetadataKey);
                ResolveControllerScopedFilter<IAutofacAuthorizationFilter, AuthorizationFilterWrapper>(
                    filterContext, m => new AuthorizationFilterWrapper(m), AuthorizationFilterMetadataKey);
                ResolveControllerScopedFilter<IAutofacExceptionFilter, ExceptionFilterWrapper>(
                    filterContext, m => new ExceptionFilterWrapper(m), ExceptionFilterMetadataKey);

                ResolveActionScopedFilter<IAutofacActionFilter, ActionFilterWrapper>(
                    filterContext, descriptor.MethodInfo, m => new ActionFilterWrapper(m), ActionFilterMetadataKey);
                ResolveActionScopedFilter<IAutofacAuthenticationFilter, AuthenticationFilterWrapper>(
                    filterContext, descriptor.MethodInfo, m => new AuthenticationFilterWrapper(m), AuthenticationFilterMetadataKey);
                ResolveActionScopedFilter<IAutofacAuthorizationFilter, AuthorizationFilterWrapper>(
                    filterContext, descriptor.MethodInfo, m => new AuthorizationFilterWrapper(m), AuthorizationFilterMetadataKey);
                ResolveActionScopedFilter<IAutofacExceptionFilter, ExceptionFilterWrapper>(
                    filterContext, descriptor.MethodInfo, m => new ExceptionFilterWrapper(m), ExceptionFilterMetadataKey);
            }

            return filters;
        }

        static void ResolveControllerScopedFilter<TFilter, TWrapper>(
            FilterContext filterContext, Func<FilterMetadata, TWrapper> wrapperFactory, string metadataKey)
            where TFilter : class
            where TWrapper : IFilter
        {
            var filters = filterContext.LifetimeScope.Resolve<IEnumerable<Meta<Lazy<TFilter>>>>();

            foreach (var filter in filters.Where(a => a.Metadata.ContainsKey(metadataKey) && a.Metadata[metadataKey] is FilterMetadata))
            {
                var metadata = (FilterMetadata)filter.Metadata[metadataKey];

                if (metadata.ControllerType != null
                    && metadata.ControllerType.IsAssignableFrom(filterContext.ControllerType)
                    && metadata.FilterScope == FilterScope.Controller
                    && metadata.MethodInfo == null
                    && !MatchingFilterAdded(filterContext.AddedFilters[metadataKey], metadata))
                {
                    var wrapper = wrapperFactory(metadata);
                    filterContext.Filters.Add(new FilterInfo(wrapper, metadata.FilterScope));
                    filterContext.AddedFilters[metadataKey].Add(metadata);
                }
            }
        }

        static void ResolveActionScopedFilter<TFilter, TWrapper>(
            FilterContext filterContext, MethodInfo methodInfo, Func<FilterMetadata, TWrapper> wrapperFactory, string metadataKey)
            where TFilter : class
            where TWrapper : IFilter
        {
            var filters = filterContext.LifetimeScope.Resolve<IEnumerable<Meta<Lazy<TFilter>>>>();

            foreach (var filter in filters.Where(a => a.Metadata.ContainsKey(metadataKey) && a.Metadata[metadataKey] is FilterMetadata))
            {
                var metadata = (FilterMetadata)filter.Metadata[metadataKey];

                if (metadata.ControllerType != null
                    && metadata.ControllerType.IsAssignableFrom(filterContext.ControllerType)
                    && metadata.FilterScope == FilterScope.Action
                    && metadata.MethodInfo.GetBaseDefinition() == methodInfo.GetBaseDefinition()
                    && !MatchingFilterAdded(filterContext.AddedFilters[metadataKey], metadata))
                {
                    var wrapper = wrapperFactory(metadata);
                    filterContext.Filters.Add(new FilterInfo(wrapper, metadata.FilterScope));
                    filterContext.AddedFilters[metadataKey].Add(metadata);
                }
            }
        }

        static bool MatchingFilterAdded(IEnumerable<FilterMetadata> filters, FilterMetadata metadata)
        {
            return filters.Any(filter => filter.ControllerType == metadata.ControllerType
                && filter.FilterScope == metadata.FilterScope
                && filter.MethodInfo == metadata.MethodInfo);
        }
    }
}