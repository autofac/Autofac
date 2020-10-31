// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Core
{
    /// <summary>
    /// Identifies a service according to a type to which it can be assigned.
    /// </summary>
    public sealed class TypedService : Service, IServiceWithType, IEquatable<TypedService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypedService"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        public TypedService(Type serviceType)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>The type of the service.</value>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets a human-readable description of the service.
        /// </summary>
        /// <value>The description.</value>
        public override string Description => ServiceType.FullName!;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(TypedService? other)
        {
            if (other == null)
            {
                return false;
            }

            return ServiceType == other.ServiceType;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="object"/> is equal to the current <see cref="object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object? obj)
        {
            return Equals(obj as TypedService);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return ServiceType.GetHashCode();
        }

        /// <summary>
        /// Return a new service of the same kind, but carrying
        /// <paramref name="newType"/> as the <see cref="ServiceType"/>.
        /// </summary>
        /// <param name="newType">The new service type.</param>
        /// <returns>A new service with the service type.</returns>
        public Service ChangeType(Type newType)
        {
            if (newType == null)
            {
                throw new ArgumentNullException(nameof(newType));
            }

            return new TypedService(newType);
        }
    }
}
