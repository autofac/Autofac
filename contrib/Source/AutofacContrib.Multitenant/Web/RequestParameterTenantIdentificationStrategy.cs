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
using System.Globalization;
using System.Web;

namespace AutofacContrib.Multitenant.Web
{
    /// <summary>
    /// Uses the specified request parameter (query string, post data etc.) to identify
    /// the current tenant.
    /// </summary>
    /// <remarks>
    /// <para>
    /// One of many potential ways to get the tenant from a request context, this
    /// strategy reads from the incoming request parameters collection (e.g.,
    /// querystring, form, etc.) and determines the tenant from there.
    /// </para>
    /// <para>
    /// Note that, due to the request parameter collection being easily modified
    /// by an outside party (simply change the querystring!), this is inherently
    /// an insecure mechanism for tenant determination. It is helpful for simple
    /// web site creation and debugging but is not recommended for production.
    /// </para>
    /// </remarks>
    public class RequestParameterTenantIdentificationStrategy : ITenantIdentificationStrategy
    {
        /// <summary>
        /// Gets the request parameter name from which the tenant ID should be retrieved.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> corresponding to a parameter in the request
        /// parameters collection indicating the tenant ID.
        /// </value>
        public string ParameterName { get; private set; }

        /// <summary>
        /// Create a new <see cref="RequestParameterTenantIdentificationStrategy"/> for
        /// the specified parameter name.
        /// </summary>
        /// <param name="parameterName">
        /// The request parameter name that holds the tenant identifier.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="parameterName" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="parameterName" /> is empty.
        /// </exception>
        public RequestParameterTenantIdentificationStrategy(string parameterName)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException("parameterName");
            }
            if (parameterName.Length == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, Properties.Resources.ArgumentException_StringEmpty, "paramterName"), "parameterName");
            }
            this.ParameterName = parameterName;
        }

        /// <summary>
        /// Attempts to identify the tenant from the request parameters.
        /// </summary>
        /// <param name="tenantId">The current tenant identifier.</param>
        /// <returns>
        /// <see langword="true"/> if the tenant could be identified; <see langword="false"/>
        /// if not.
        /// </returns>
        public bool TryIdentifyTenant(out object tenantId)
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                tenantId = null;
                return false;
            }

            tenantId = context.Request.Params[this.ParameterName];
            return tenantId != null;
        }
    }
}
