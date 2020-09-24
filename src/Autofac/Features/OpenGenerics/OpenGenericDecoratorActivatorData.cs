// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.OpenGenerics
{
    /// <summary>
    /// Describes the activator for an open generic decorator.
    /// </summary>
    public class OpenGenericDecoratorActivatorData : ReflectionActivatorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenGenericDecoratorActivatorData"/> class.
        /// </summary>
        /// <param name="implementer">The decorator type.</param>
        /// <param name="fromService">The open generic service type to decorate.</param>
        public OpenGenericDecoratorActivatorData(Type implementer, IServiceWithType fromService)
            : base(implementer)
        {
            if (fromService == null)
            {
                throw new ArgumentNullException(nameof(fromService));
            }

            if (!fromService.ServiceType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericDecoratorActivatorDataResources.DecoratedServiceIsNotOpenGeneric, fromService));
            }

            FromService = fromService;
        }

        /// <summary>
        /// Gets the open generic service type to decorate.
        /// </summary>
        public IServiceWithType FromService { get; }
    }
}
