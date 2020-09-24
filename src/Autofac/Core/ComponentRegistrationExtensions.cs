// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core.Lifetime;

namespace Autofac.Core
{
    /// <summary>
    /// Extension methods for <see cref="IComponentRegistration"/>.
    /// </summary>
    public static class ComponentRegistrationExtensions
    {
        /// <summary>
        /// For components registered instance-per-matching-lifetime-scope, retrieves the set
        /// of lifetime scope tags to match.
        /// </summary>
        /// <param name="registration">
        /// The <see cref="IComponentRegistration"/> to query for matching lifetime scope tags.
        /// </param>
        /// <returns>
        /// If the component is registered instance-per-matching-lifetime-scope, this method returns
        /// the set of matching lifetime scope tags. If the component is singleton, instance-per-scope,
        /// instance-per-dependency, or otherwise not an instance-per-matching-lifetime-scope
        /// component, this method returns an empty enumeration.
        /// </returns>
        public static IEnumerable<object> MatchingLifetimeScopeTags(this IComponentRegistration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            if (registration.Lifetime is MatchingScopeLifetime lifetime)
            {
                return lifetime.TagsToMatch;
            }

            return Enumerable.Empty<object>();
        }
    }
}
