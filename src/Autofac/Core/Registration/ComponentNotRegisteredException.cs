// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// A service was requested that cannot be provided by the container. To avoid this exception, either register a component
    /// to provide the required service, check for service registration using IsRegistered(), or use the ResolveOptional()
    /// method to resolve an optional dependency.
    /// </summary>
    /// <remarks>This exception is fatal. See <see cref="DependencyResolutionException"/> for more information.</remarks>
    public class ComponentNotRegisteredException : DependencyResolutionException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNotRegisteredException"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public ComponentNotRegisteredException(Service service)
            : base(FormatMessage(service))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentNotRegisteredException"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="innerException">The inner exception.</param>
        public ComponentNotRegisteredException(Service service, Exception innerException)
            : base(FormatMessage(service), innerException)
        {
        }

        private static string FormatMessage(Service service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return string.Format(CultureInfo.CurrentCulture, ComponentNotRegisteredExceptionResources.Message, service);
        }
    }
}
