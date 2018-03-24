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
using System.Linq;

namespace Autofac.Features.Decorators
{
    public sealed class DecoratorContext : IDecoratorContext
    {
        /// <inheritdoc />
        public Type ImplementationType { get; private set; }

        /// <inheritdoc />
        public Type ServiceType { get; private set; }

        /// <inheritdoc />
        public IEnumerable<Type> AppliedDecoratorTypes { get; private set; }

        /// <inheritdoc />
        public IEnumerable<object> AppliedDecorators { get; private set; }

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
                AppliedDecorators = Enumerable.Empty<object>(),
                AppliedDecoratorTypes = Enumerable.Empty<Type>(),
                CurrentInstance = implementationInstance
            };
            return context;
        }

        internal DecoratorContext UpdateContext(object decoratorInstance)
        {
            var context = new DecoratorContext
            {
                ImplementationType = ImplementationType,
                ServiceType = ServiceType,
                AppliedDecorators = AppliedDecorators.Concat(new[] { decoratorInstance }),
                AppliedDecoratorTypes = AppliedDecoratorTypes.Concat(new[] { decoratorInstance.GetType() }),
                CurrentInstance = decoratorInstance
            };
            return context;
        }
    }
}
