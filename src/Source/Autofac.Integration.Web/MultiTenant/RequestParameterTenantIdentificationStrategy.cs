// Copyright (c) 2010 Autofac Contributors
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
using System.Web;

namespace Autofac.Integration.Web.MultiTenant
{
    /// <summary>
    /// Uses the specified request parameter (query string, post data etc.) to identify
    /// the current tenant.
    /// </summary>
    /// <remarks>This strategy is inherently non-secure.</remarks>
    public class RequestParameterTenantIdentificationStrategy : ITenantIdentificationPolicy
    {
        readonly string _parameterName;

        /// <summary>
        /// Create a new <see cref="RequestParameterTenantIdentificationStrategy"/> for
        /// the specified parameter name.
        /// </summary>
        /// <param name="parameterName">The request parameter name that holds the
        /// tenant identifier.</param>
        public RequestParameterTenantIdentificationStrategy(string parameterName)
        {
            if (parameterName == null) throw new ArgumentNullException("parameterName");
            _parameterName = parameterName;
        }

        /// <summary>
        /// Identify the current tenant, if any.
        /// </summary>
        /// <param name="tenantId">The tenant identifier, if any is available.</param>
        /// <returns>True if the current tenant could be identified; otherwise, false.</returns>
        public bool TryIdentifyTenant(out object tenantId)
        {
            tenantId = HttpContext.Current.Request.Params[_parameterName];
            return tenantId != null;
        }
    }
}