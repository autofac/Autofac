// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            _delegateType = delegateType ?? throw new ArgumentNullException(nameof(delegateType));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
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
