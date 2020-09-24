// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Autofac.Builder;

namespace Autofac.Features.AttributeFilters
{
    /// <summary>
    /// Extends registration syntax for attribute scenarios.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Applies attribute-based filtering on constructor dependencies for use with attributes
        /// derived from the <see cref="ParameterFilterAttribute"/>.
        /// </summary>
        /// <typeparam name="TLimit">The type of the registration limit.</typeparam>
        /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
        /// <param name="builder">The registration builder containing registration data.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Apply this extension to component registrations that use attributes
        /// that derive from the <see cref="ParameterFilterAttribute"/>
        /// like the <see cref="MetadataFilterAttribute"/>
        /// in their constructors. Doing so will allow the attribute-based filtering to occur. See
        /// <see cref="MetadataFilterAttribute"/> for an
        /// example on how to use the filter and attribute together.
        /// </para>
        /// </remarks>
        /// <seealso cref="MetadataFilterAttribute"/>
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TRegistrationStyle>
            WithAttributeFiltering<TLimit, TReflectionActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TReflectionActivatorData, TRegistrationStyle> builder)
            where TReflectionActivatorData : ReflectionActivatorData
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.WithParameter(
                (p, c) =>
                {
                    var filter = p.GetCustomAttributes<ParameterFilterAttribute>(true).FirstOrDefault();
                    return filter != null && filter.CanResolveParameter(p, c);
                },
                (p, c) =>
                {
                    var filter = p.GetCustomAttributes<ParameterFilterAttribute>(true).First();
                    return filter.ResolveParameter(p, c);
                });
        }
    }
}
