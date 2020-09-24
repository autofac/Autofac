// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
