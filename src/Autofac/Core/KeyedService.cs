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

namespace Autofac.Core
{
    /// <summary>
    /// Identifies a service using a key in addition to its type.
    /// </summary>
    public sealed class KeyedService : Service, IServiceWithType, IEquatable<KeyedService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Autofac.Core.KeyedService"/> class.
        /// </summary>
        /// <param name="serviceKey">Key of the service.</param>
        /// <param name="serviceType">Type of the service.</param>
        public KeyedService(object serviceKey, Type serviceType)
        {
            if (serviceKey == null) throw new ArgumentNullException(nameof(serviceKey));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            ServiceKey = serviceKey;
            ServiceType = serviceType;
        }

        /// <summary>
        /// Gets the key of the service.
        /// </summary>
        /// <value>The key of the service.</value>
        public object ServiceKey { get; }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>The type of the service.</value>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets a human-readable description of the service.
        /// </summary>
        /// <value>The description.</value>
        public override string Description => ServiceKey + " (" + ServiceType.FullName + ")";

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(KeyedService other)
        {
            if (other == null)
                return false;

            return ServiceKey.Equals(other.ServiceKey) && ServiceType == other.ServiceType;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="System.Object"/> is equal to the current <see cref="System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            return Equals(obj as KeyedService);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return ServiceKey.GetHashCode() ^ ServiceType.GetHashCode();
        }

        /// <summary>
        /// Return a new service of the same kind, but carrying
        /// <paramref name="newType"/> as the <see cref="ServiceType"/>.
        /// </summary>
        /// <param name="newType">The new service type.</param>
        /// <returns>A new service with the service type.</returns>
        public Service ChangeType(Type newType)
        {
            if (newType == null) throw new ArgumentNullException(nameof(newType));

            return new KeyedService(ServiceKey, newType);
        }
    }
}
