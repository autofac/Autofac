// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;

namespace Autofac.Features.OwnedInstances
{
    /// <summary>
    /// Defines a key for a matching lifetime scope used by instance-per-owned.
    /// </summary>
    internal class InstancePerOwnedKey : IEquatable<IServiceWithType>
    {
        private readonly TypedService _serviceWithType;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstancePerOwnedKey"/> class.
        /// </summary>
        /// <param name="typedService">The encapsulated service.</param>
        public InstancePerOwnedKey(TypedService typedService)
            => _serviceWithType = typedService;

        /// <inheritdoc/>
        public bool Equals(IServiceWithType other)
            => other != null && _serviceWithType.ServiceType == other.ServiceType;

        /// <inheritdoc/>
        public override bool Equals(object obj)
            => obj is IServiceWithType serviceWithType && Equals(serviceWithType);

        /// <inheritdoc/>
        public override int GetHashCode()
            => _serviceWithType.ServiceType.GetHashCode();
    }
}
