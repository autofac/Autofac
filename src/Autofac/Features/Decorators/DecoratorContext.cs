// This software is part of the Autofac IoC container
// Copyright © 2018 Autofac Contributors
// https://autofac.org
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
            AppliedDecoratorTypes = appliedDecoratorTypes ?? new List<Type>(0);
            AppliedDecorators = appliedDecorators ?? new List<object>(0);
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
