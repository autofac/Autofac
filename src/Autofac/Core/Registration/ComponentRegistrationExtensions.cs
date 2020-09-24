// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Extension methods for component registrations.
    /// </summary>
    internal static class ComponentRegistrationExtensions
    {
        /// <summary>
        /// Gets a value indicating whether a given registration is adapting another registration.
        /// </summary>
        /// <param name="componentRegistration">The component registration.</param>
        /// <returns>True if adapting; false otherwise.</returns>
        public static bool IsAdapting(this IComponentRegistration componentRegistration)
        {
            return componentRegistration.Target != componentRegistration;
        }
    }
}
