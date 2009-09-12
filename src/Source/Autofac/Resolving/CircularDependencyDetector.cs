// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.Text;
using System.Globalization;

namespace Autofac.Resolving
{
    class CircularDependencyDetector
    {
        /// <summary>
        /// Catch circular dependencies that are triggered by post-resolve processing (e.g. 'OnActivated')
        /// </summary>
        const int MaxResolveDepth = 100;

        string CreateDependencyGraphTo(IComponentRegistration registration, Stack<ComponentActivation> activationStack)
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(activationStack, "activationStack");

            string dependencyGraph = registration.ToString();

            foreach (IComponentRegistration requestor in activationStack.Select(a => a.Registration))
                dependencyGraph = requestor.ToString() + " -> " + dependencyGraph;

            return dependencyGraph;
        }

        public void CheckForCircularDependency(IComponentRegistration registration, Stack<ComponentActivation> activationStack, int callDepth)
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(activationStack, "activationStack");

            if (callDepth > MaxResolveDepth)
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture,
                    CircularDependencyDetectorResources.MaxDepthExceeded, registration));

            if (IsCircularDependency(registration, activationStack))
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture,
                    CircularDependencyDetectorResources.CircularDependency, CreateDependencyGraphTo(registration, activationStack)));
        }

        bool IsCircularDependency(IComponentRegistration registration, Stack<ComponentActivation> activationStack)
        {
            Enforce.ArgumentNotNull(registration, "registration");
            Enforce.ArgumentNotNull(activationStack, "activationStack");
            return activationStack.Count(a => a.Registration == registration) >= 1;
        }
    }
}
