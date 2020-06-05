// This software is part of the Autofac IoC container
// Copyright © 2019 Autofac Contributors
// https://autofac.org
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

using System.Linq;
using Autofac.Core;

namespace Autofac.Builder
{
    /// <summary>
    /// Provides support for accessing/invoking the set of build callbacks, invoked on scope/container build.
    /// </summary>
    internal static class BuildCallbackManager
    {
        private static readonly TypedService CallbackServiceType = new TypedService(typeof(BuildCallbackService));

        private const string BuildCallbacksExecutedKey = nameof(BuildCallbacksExecutedKey);

        /// <summary>
        /// Executes the newly-registered build callbacks for a given scope/container..
        /// </summary>
        /// <param name="scope">The new scope/container.</param>
        internal static void RunBuildCallbacks(ILifetimeScope scope)
        {
            var buildCallbackServices = scope.ComponentRegistry.ServiceRegistrationsFor(CallbackServiceType);

            foreach (var srv in buildCallbackServices)
            {
                // Use the metadata to track whether we've executed already, to avoid issuing a resolve request.
                if (srv.Metadata.ContainsKey(BuildCallbacksExecutedKey))
                {
                    continue;
                }

                var request = new ResolveRequest(CallbackServiceType, srv, Enumerable.Empty<Parameter>());
                var component = (BuildCallbackService)scope.ResolveComponent(request);
                srv.Registration.Metadata[BuildCallbacksExecutedKey] = true;

                // Run the callbacks for the relevant scope.
                component.Execute(scope);
            }
        }
    }
}
