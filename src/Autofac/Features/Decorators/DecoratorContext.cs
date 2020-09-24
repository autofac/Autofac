// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Autofac.Features.Decorators
{
    /// <summary>
    /// Implements the decorator context, exposing the state of the decoration process.
    /// </summary>
    public sealed class DecoratorContext : IDecoratorContext
    {
        /// <inheritdoc />
        public Type ImplementationType { get; private set; }

        /// <inheritdoc />
        public Type ServiceType { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<Type> AppliedDecoratorTypes { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<object> AppliedDecorators { get; private set; }

        /// <inheritdoc />
        public object CurrentInstance { get; private set; }

        private DecoratorContext(
            Type implementationType,
            Type serviceType,
            object currentInstance,
            IReadOnlyList<Type>? appliedDecoratorTypes = null,
            IReadOnlyList<object>? appliedDecorators = null)
        {
            ImplementationType = implementationType;
            ServiceType = serviceType;
            CurrentInstance = currentInstance;
            AppliedDecoratorTypes = appliedDecoratorTypes ?? Array.Empty<Type>();
            AppliedDecorators = appliedDecorators ?? Array.Empty<object>();
        }

        /// <summary>
        /// Create a new <see cref="DecoratorContext"/>.
        /// </summary>
        /// <param name="implementationType">The type of the concrete implementation.</param>
        /// <param name="serviceType">The service type being decorated.</param>
        /// <param name="implementationInstance">The instance of the implementation to be decorated.</param>
        /// <returns>A new decorator context.</returns>
        internal static DecoratorContext Create(Type implementationType, Type serviceType, object implementationInstance)
        {
            return new DecoratorContext(implementationType, serviceType, implementationInstance);
        }

        /// <summary>
        /// Creates a new decorator context that consumes the specified decorator instance.
        /// </summary>
        /// <param name="decoratorInstance">The decorated instance.</param>
        /// <returns>A new context.</returns>
        internal DecoratorContext UpdateContext(object decoratorInstance)
        {
            var appliedDecorators = new List<object>(AppliedDecorators.Count + 1);
            appliedDecorators.AddRange(AppliedDecorators);
            appliedDecorators.Add(decoratorInstance);

            var appliedDecoratorTypes = new List<Type>(AppliedDecoratorTypes.Count + 1);
            appliedDecoratorTypes.AddRange(AppliedDecoratorTypes);
            appliedDecoratorTypes.Add(decoratorInstance.GetType());

            return new DecoratorContext(ImplementationType, ServiceType, decoratorInstance, appliedDecoratorTypes, appliedDecorators);
        }
    }
}
