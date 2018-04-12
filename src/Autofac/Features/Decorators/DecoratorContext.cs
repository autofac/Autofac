// This software is part of the Autofac IoC container
// Copyright © 2018 Autofac Contributors
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
using System.Collections.Generic;

namespace Autofac.Features.Decorators
{
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

        private DecoratorContext()
        {
        }

        internal static DecoratorContext Create(Type implementationType, Type serviceType, object implementationInstance)
        {
            var context = new DecoratorContext
            {
                ImplementationType = implementationType,
                ServiceType = serviceType,
                AppliedDecorators = new List<object>(0),
                AppliedDecoratorTypes = new List<Type>(0),
                CurrentInstance = implementationInstance
            };

            return context;
        }

        internal DecoratorContext UpdateContext(object decoratorInstance)
        {
            var appliedDecorators = new List<object>(AppliedDecorators.Count + 1);
            appliedDecorators.AddRange(AppliedDecorators);
            appliedDecorators.Add(decoratorInstance);

            var appliedDecoratorTypes = new List<Type>(AppliedDecoratorTypes.Count + 1);
            appliedDecoratorTypes.AddRange(AppliedDecoratorTypes);
            appliedDecoratorTypes.Add(decoratorInstance.GetType());

            var context = new DecoratorContext
            {
                ImplementationType = ImplementationType,
                ServiceType = ServiceType,
                AppliedDecorators = appliedDecorators,
                AppliedDecoratorTypes = appliedDecoratorTypes,
                CurrentInstance = decoratorInstance
            };

            return context;
        }
    }
}
