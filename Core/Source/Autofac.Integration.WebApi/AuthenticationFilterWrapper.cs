// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
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
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Resolves a filter for the specified metadata for each controller request.
    /// </summary>
    [SecurityCritical]
    internal class AuthenticationFilterWrapper : IAuthenticationFilter, IAutofacAuthenticationFilter
    {
        readonly FilterMetadata _filterMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationFilterWrapper"/> class.
        /// </summary>
        /// <param name="filterMetadata">The filter metadata.</param>
        public AuthenticationFilterWrapper(FilterMetadata filterMetadata)
        {
            if (filterMetadata == null) throw new ArgumentNullException("filterMetadata");

            _filterMetadata = filterMetadata;
        }

        [SecurityCritical]
        public void OnAuthenticate(HttpAuthenticationContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var dependencyScope = context.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacAuthenticationFilter>>>>();

            foreach (var filter in filters.Where(FilterMatchesMetadata))
                filter.Value.Value.OnAuthenticate(context);
        }

        [SecurityCritical]
        public void OnChallenge(HttpAuthenticationChallengeContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var dependencyScope = context.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacAuthenticationFilter>>>>();

            foreach (var filter in filters.Where(FilterMatchesMetadata))
                filter.Value.Value.OnChallenge(context);
        }

        bool IFilter.AllowMultiple
        {
            [SecurityCritical]
            get { return true; }
        }

        [SecurityCritical]
        Task IAuthenticationFilter.AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnAuthenticate(context), cancellationToken);
        }

        [SecurityCritical]
        Task IAuthenticationFilter.ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnChallenge(context), cancellationToken);
        }

        bool FilterMatchesMetadata(Meta<Lazy<IAutofacAuthenticationFilter>> filter)
        {
            var metadata = filter.Metadata[AutofacWebApiFilterProvider.AuthenticationFilterMetadataKey] as FilterMetadata;

            return metadata != null
                   && metadata.ControllerType == _filterMetadata.ControllerType
                   && metadata.FilterScope == _filterMetadata.FilterScope
                   && metadata.MethodInfo == _filterMetadata.MethodInfo;
        }
    }
}