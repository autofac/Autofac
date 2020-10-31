// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;

namespace Autofac.Features.Decorators
{
    /// <summary>
    /// A service that has been registered for the purpose of decorating other components.
    /// </summary>
    /// <remarks>
    /// This service type is used to locate decorator services and is intended for internal use only.
    /// </remarks>
    public sealed class DecoratorService : Service, IServiceWithType, IEquatable<DecoratorService>
    {
        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the condition that must be met for the decorator to be applied.
        /// </summary>
        public Func<IDecoratorContext, bool> Condition { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoratorService"/> class.
        /// </summary>
        /// <param name="serviceType">The service type for the decorator.</param>
        /// <param name="condition">The condition that must be met for the decorator to be applied.</param>
        public DecoratorService(Type serviceType, Func<IDecoratorContext, bool>? condition = null)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Condition = condition ?? (context => true);
        }

        /// <inheritdoc />
        public override string Description => $"Decorator ({ServiceType.FullName})";

        /// <inheritdoc />
        public Service ChangeType(Type newType)
        {
            if (newType == null)
            {
                throw new ArgumentNullException(nameof(newType));
            }

            return new DecoratorService(newType, Condition);
        }

        /// <inheritdoc />
        public bool Equals(DecoratorService? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ServiceType == other.ServiceType;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((DecoratorService)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (GetType().GetHashCode() * 397) ^ ServiceType.GetHashCode();
            }
        }
    }
}
