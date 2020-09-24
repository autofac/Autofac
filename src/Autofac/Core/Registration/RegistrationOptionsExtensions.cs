// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Extension methods for registration options.
    /// </summary>
    public static class RegistrationOptionsExtensions
    {
        /// <summary>
        /// Tests whether a given flag (or combined set of flags) is present in the specified
        /// options enumeration.
        /// </summary>
        /// <param name="options">The option to test.</param>
        /// <param name="flag">The flag (or flags) to test for.</param>
        /// <returns>True if the specified flag (or flags) are enabled for the registration.</returns>
        public static bool HasOption(this RegistrationOptions options, RegistrationOptions flag)
        {
            return (options & flag) == flag;
        }
    }
}
