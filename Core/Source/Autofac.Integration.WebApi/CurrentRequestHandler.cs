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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    ///     A delegating handler that updates the current dependency scope
    ///     with the current <see cref="HttpRequestMessage"/>.
    /// </summary>
    class CurrentRequestHandler : DelegatingHandler
    {
        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.
        /// </returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UpdateScopeWithHttpRequestMessage(request);

            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Updates the current dependency scope with current HTTP request message.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        internal static void UpdateScopeWithHttpRequestMessage(HttpRequestMessage request)
        {
            var scope = request.GetDependencyScope();
            var requestScope = scope.GetRequestLifetimeScope();
            if (requestScope == null) return;

            var registry = requestScope.ComponentRegistry;
            var builder = new ContainerBuilder();
            builder.Register(c => request).InstancePerApiRequest();
            builder.Update(registry);
        }
    }
}
