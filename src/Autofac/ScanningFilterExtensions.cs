// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Features.Scanning;

namespace Autofac
{
    /// <summary>
    /// Convenience filters for use with assembly scanning registrations.
    /// </summary>
    public static class ScanningFilterExtensions
    {
        /// <summary>
        /// Filters scanned assembly types to be only the public types.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <param name="registration">Registration to filter types from.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            PublicOnly<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration)
            where TScanningActivatorData : ScanningActivatorData
        {
            // Issue #897: Back compat dictates we can't disable non-public types
            // from being found by default, but this convenience method will allow
            // people to opt in.
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.Where(t => t.IsPublic || t.IsNestedPublic);
        }
    }
}
