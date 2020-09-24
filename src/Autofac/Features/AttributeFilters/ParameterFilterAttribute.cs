// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;

namespace Autofac.Features.AttributeFilters
{
    /// <summary>
    /// Base attribute class for marking constructor parameters and enabling
    /// filtering by attributed criteria.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementations of this attribute can be used to mark constructor parameters
    /// so filtering can be done based on registered service data. For example, the
    /// <see cref="MetadataFilterAttribute"/> allows constructor
    /// parameters to be filtered by registered metadata criteria and the
    /// <see cref="KeyFilterAttribute"/> allows constructor
    /// parameters to be filtered by a keyed service registration.
    /// </para>
    /// <para>
    /// If a type uses these attributes, it should be registered with Autofac
    /// using the
    /// <see cref="RegistrationExtensions.WithAttributeFiltering{TLimit, TReflectionActivatorData, TStyle}" />
    /// extension to enable the behavior.
    /// </para>
    /// <para>
    /// For specific attribute usage examples, see the attribute documentation.
    /// </para>
    /// </remarks>
    /// <seealso cref="MetadataFilterAttribute"/>
    /// <seealso cref="KeyFilterAttribute"/>
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class ParameterFilterAttribute : Attribute
    {
        /// <summary>
        /// Implemented in derived classes to resolve a specific parameter marked with this attribute.
        /// </summary>
        /// <param name="parameter">The specific parameter being resolved that is marked with this attribute.</param>
        /// <param name="context">The component context under which the parameter is being resolved.</param>
        /// <returns>The instance of the object that should be used for the parameter value.</returns>
        public abstract object? ResolveParameter(ParameterInfo parameter, IComponentContext context);

        /// <summary>
        /// Implemented in derived classes to check a specific parameter can be resolved.
        /// </summary>
        /// <param name="parameter">The specific parameter being resolved that is marked with this attribute.</param>
        /// <param name="context">The component context under which the parameter is being resolved.</param>
        /// <returns>true if parameter can be resolved; otherwise, false.</returns>
        public abstract bool CanResolveParameter(ParameterInfo parameter, IComponentContext context);
    }
}
