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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Autofac.Core.Resolving
{
    internal class CircularDependencyDetector
    {
        /// <summary>
        /// Catch circular dependencies that are triggered by post-resolve processing (e.g. 'OnActivated').
        /// </summary>
        [SuppressMessage("SA1306", "SA1306", Justification = "Changed const to static on temporary basis until we can solve circular dependencies without a limit.")]
        private static int MaxResolveDepth = 50;

        private static string CreateDependencyGraphTo(IComponentRegistration registration, Stack<InstanceLookup> activationStack)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (activationStack == null) throw new ArgumentNullException(nameof(activationStack));

            var dependencyGraph = Display(registration);

            return activationStack.Select(a => a.ComponentRegistration)
                .Aggregate(dependencyGraph, (current, requestor) => Display(requestor) + " -> " + current);
        }

        private static string Display(IComponentRegistration registration)
        {
            return registration.Activator.LimitType.FullName ?? string.Empty;
        }

        public static void CheckForCircularDependency(IComponentRegistration registration, Stack<InstanceLookup> activationStack, int callDepth)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            if (callDepth > MaxResolveDepth)
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, CircularDependencyDetectorResources.MaxDepthExceeded, registration));

            // Checks for circular dependency
            foreach (var a in activationStack)
            {
                if (a.ComponentRegistration == registration)
                {
                    throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, CircularDependencyDetectorResources.CircularDependency, CreateDependencyGraphTo(registration, activationStack)));
                }
            }
        }
    }
}
