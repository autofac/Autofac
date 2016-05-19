// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;

namespace Autofac.Features.GeneratedFactories
{
    /// <summary>
    /// Data used to create factory activators.
    /// </summary>
    public class GeneratedFactoryActivatorData : IConcreteActivatorData
    {
        private readonly Type _delegateType;
        private readonly Service _productService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedFactoryActivatorData"/> class.
        /// </summary>
        /// <param name="delegateType">The type of the factory.</param>
        /// <param name="productService">The service used to provide the products of the factory.</param>
        public GeneratedFactoryActivatorData(Type delegateType, Service productService)
        {
            if (delegateType == null) throw new ArgumentNullException(nameof(delegateType));
            if (productService == null) throw new ArgumentNullException(nameof(productService));

            _delegateType = delegateType;
            _productService = productService;
        }

        /// <summary>
        /// Gets or sets a value determining how the parameters of the delegate type are passed on
        /// to the generated Resolve() call as Parameter objects.
        /// For Func-based delegates, this defaults to ByType. Otherwise, the
        /// parameters will be mapped by name.
        /// </summary>
        public ParameterMapping ParameterMapping { get; set; } = ParameterMapping.Adaptive;

        /// <summary>
        /// Gets the activator data that can provide an IInstanceActivator instance.
        /// </summary>
        public IInstanceActivator Activator
        {
            get
            {
                var factory = new FactoryGenerator(_delegateType, _productService, ParameterMapping);
                return new DelegateActivator(_delegateType, (c, p) => factory.GenerateFactory(c, p));
            }
        }
    }
}
