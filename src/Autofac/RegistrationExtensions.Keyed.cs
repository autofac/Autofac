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
using System.Diagnostics.CodeAnalysis;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
    {
        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a keyed service.
        /// </summary>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <typeparam name="TService">Service type provided by the component.</typeparam>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            Keyed<TService>(
                this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration,
                Func<Type, object> serviceKeyMapping)
        {
            return Keyed(registration, serviceKeyMapping, typeof(TService));
        }

        /// <summary>
        /// Specifies how a type from a scanned assembly is mapped to a keyed service.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to set service mapping on.</param>
        /// <param name="serviceType">Service type provided by the component.</param>
        /// <param name="serviceKeyMapping">Function mapping types to service keys.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            Keyed<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Func<Type, object> serviceKeyMapping,
                Type serviceType)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (serviceKeyMapping == null)
            {
                throw new ArgumentNullException(nameof(serviceKeyMapping));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            return registration
                .AssignableTo(serviceType)
                .As(t => new KeyedService(serviceKeyMapping(t), serviceType));
        }
    }
}
