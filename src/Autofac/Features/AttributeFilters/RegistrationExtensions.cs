// This software is part of the Autofac IoC container
// Copyright © 2013 Autofac Contributors
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
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithParameter(
                (p, c) => p.GetCustomAttributes<ParameterFilterAttribute>(true).Any(),
                (p, c) =>
                {
                    var filter = p.GetCustomAttributes<ParameterFilterAttribute>(true).First();
                    return filter.ResolveParameter(p, c);
                });
        }
    }
}
