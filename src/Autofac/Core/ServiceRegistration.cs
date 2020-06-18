// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
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
using Autofac.Builder;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core
{
    /// <summary>
    /// Defines a combination of a service pipeline and a registration. Used to instantiate a <see cref="ResolveRequest"/>.
    /// </summary>
    public struct ServiceRegistration : IEquatable<ServiceRegistration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistration"/> struct.
        /// </summary>
        /// <param name="servicePipeline">A service pipeline.</param>
        /// <param name="registration">The registration.</param>
        public ServiceRegistration(IResolvePipeline servicePipeline, IComponentRegistration registration)
        {
            Pipeline = servicePipeline ?? throw new ArgumentNullException(nameof(servicePipeline));
            Registration = registration ?? throw new ArgumentNullException(nameof(registration));
        }

        /// <summary>
        /// Gets the pipeline to invoke that will resolve the associated <see cref="Registration"/>.
        /// </summary>
        public IResolvePipeline Pipeline { get; }

        /// <summary>
        /// Gets the registration that will be resolved when a resolve request runs.
        /// </summary>
        public IComponentRegistration Registration { get; }

        /// <summary>
        /// Gets additional data associated with the component.
        /// </summary>
        public IDictionary<string, object?> Metadata => Registration.Metadata;

        /// <summary>
        /// Gets the registration order value from the registration.
        /// </summary>
        /// <returns>The original registration order value.</returns>
        public long GetRegistrationOrder() => Registration.GetRegistrationOrder();

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ServiceRegistration registration && Equals(registration);
        }

        /// <inheritdoc/>
        public bool Equals(ServiceRegistration other)
        {
            return Pipeline == other.Pipeline && Registration == other.Registration;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (Pipeline is null || Registration is null)
            {
                return 0;
            }

            return Pipeline.GetHashCode() ^ Registration.GetHashCode();
        }

        public static bool operator ==(ServiceRegistration left, ServiceRegistration right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ServiceRegistration left, ServiceRegistration right)
        {
            return !(left == right);
        }
    }
}
