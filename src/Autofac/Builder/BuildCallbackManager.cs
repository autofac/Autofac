// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
