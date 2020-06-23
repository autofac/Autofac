// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core
{
    /// <summary>
    /// Describes a logical component within the container.
    /// </summary>
    public interface IComponentRegistration : IDisposable
    {
        /// <summary>
        /// Gets a unique identifier for this component (shared in all sub-contexts.)
        /// This value also appears in Services.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the activator used to create instances.
        /// </summary>
        IInstanceActivator Activator { get; }

        /// <summary>
        /// Gets the lifetime associated with the component.
        /// </summary>
        IComponentLifetime Lifetime { get; }

        /// <summary>
        /// Gets a value indicating whether the component instances are shared or not.
        /// </summary>
        InstanceSharing Sharing { get; }

        /// <summary>
        /// Gets a value indicating whether the instances of the component should be disposed by the container.
        /// </summary>
        InstanceOwnership Ownership { get; }

        /// <summary>
        /// Gets the services provided by the component.
        /// </summary>
        IEnumerable<Service> Services { get; }

        /// <summary>
        /// Gets additional data associated with the component.
        /// </summary>
        IDictionary<string, object?> Metadata { get; }

        /// <summary>
        /// Gets the component registration upon which this registration is based.
        /// </summary>
        IComponentRegistration Target { get; }

        /// <summary>
        /// Gets the resolve pipeline for the component.
        /// </summary>
        IResolvePipeline ResolvePipeline { get; }

        /// <summary>
        /// Gets the options for the registration.
        /// </summary>
        RegistrationOptions Options { get; }

        /// <summary>
        /// Provides an event that will be invoked just before a pipeline is built, and can be used to add additional middleware
        /// at that point.
        /// </summary>
        public event EventHandler<IResolvePipelineBuilder> PipelineBuilding;

        /// <summary>
        /// Builds the resolve pipeline.
        /// </summary>
        /// <param name="registryServices">The available services.</param>
        void BuildResolvePipeline(IComponentRegistryServices registryServices);
    }
}
