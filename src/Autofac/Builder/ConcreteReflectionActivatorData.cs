// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Builder
{
    /// <summary>
    /// Reflection activator data for concrete types.
    /// </summary>
    public class ConcreteReflectionActivatorData : ReflectionActivatorData, IConcreteActivatorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcreteReflectionActivatorData"/> class.
        /// </summary>
        /// <param name="implementer">Type that will be activated.</param>
        public ConcreteReflectionActivatorData(Type implementer)
            : base(implementer)
        {
        }

        /// <summary>
        /// Gets the instance activator based on the provided data.
        /// </summary>
        public IInstanceActivator Activator => new ReflectionActivator(
            ImplementationType,
            ConstructorFinder,
            ConstructorSelector,
            ConfiguredParameters,
            ConfiguredProperties);
    }
}
