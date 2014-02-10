// This software is part of the Autofac IoC container
// Copyright © 2014 Autofac Contributors
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security;
using System.Web.Http;
using Autofac.Integration.Owin;
using Owin;

namespace Autofac.Integration.WebApi.Owin
{
    /// <summary>
    /// Extension methods for configuring the OWIN pipeline.
    /// </summary>
    [SecuritySafeCritical]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OwinExtensions
    {
        /// <summary>
        /// Extends the Autofac lifetime scope added from the OWIN pipeline through to the Web API dependency scope.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="configuration">The HTTP server configuration.</param>
        /// <returns>The application builder.</returns>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static IAppBuilder UseAutofacWebApi(this IAppBuilder app, HttpConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            if (!configuration.MessageHandlers.OfType<DependencyScopeHandler>().Any())
                configuration.MessageHandlers.Insert(0, new DependencyScopeHandler());

            return app;
        }
    }
}
