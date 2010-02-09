// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
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
using Autofac.Util;

namespace Autofac.Core
{
    /// <summary>
    /// Identifies a service according to a type to which it can be assigned.
    /// </summary>
    public sealed class TypedService : Service, IServiceWithType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypedService"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        public TypedService(Type serviceType)
        {
            ServiceType = Enforce.ArgumentNotNull(serviceType, "serviceType");
        }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>The type of the service.</value>
        public Type ServiceType { get; private set; }

        /// <summary>
        /// Gets a human-readable description of the service.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {    
                return ServiceType.FullName;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            TypedService that = obj as TypedService;

            if (that == null)
                return false;

            return ServiceType == that.ServiceType;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
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
            Enforce.ArgumentNotNull(newType, "newType");
            return new TypedService(newType);
        }
    }
}
