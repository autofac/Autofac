// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace Autofac.Builder
{
    /// <summary>
    /// Builder for reflection-based activators.
    /// </summary>
    public class ReflectionActivatorData
    {
        private Type _implementer = default!;
        private IConstructorFinder _constructorFinder;
        private IConstructorSelector _constructorSelector;

        private static readonly IConstructorFinder DefaultConstructorFinder = new DefaultConstructorFinder();
        private static readonly IConstructorSelector DefaultConstructorSelector = new MostParametersConstructorSelector();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionActivatorData"/> class.
        /// </summary>
        /// <param name="implementer">Type that will be activated.</param>
        public ReflectionActivatorData(Type implementer)
        {
            ImplementationType = implementer;

            _constructorFinder = DefaultConstructorFinder;
            _constructorSelector = DefaultConstructorSelector;
        }

        /// <summary>
        /// Gets or sets the implementation type.
        /// </summary>
        public Type ImplementationType
        {
            get
            {
                return _implementer;
            }

            set
            {
                _implementer = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the constructor finder for the registration.
        /// </summary>
        public IConstructorFinder ConstructorFinder
        {
            get
            {
                return _constructorFinder;
            }

            set
            {
                _constructorFinder = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the constructor selector for the registration.
        /// </summary>
        public IConstructorSelector ConstructorSelector
        {
            get
            {
                return _constructorSelector;
            }

            set
            {
                _constructorSelector = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets the explicitly bound constructor parameters.
        /// </summary>
        public IList<Parameter> ConfiguredParameters { get; } = new List<Parameter>();

        /// <summary>
        /// Gets the explicitly bound properties.
        /// </summary>
        public IList<Parameter> ConfiguredProperties { get; } = new List<Parameter>();
    }
}
