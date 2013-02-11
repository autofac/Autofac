// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
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
using NHibernate.Bytecode;
using NHibernate.Properties;

namespace Autofac.Extras.NHibernate.Bytecode
{
    /// <summary>
    /// Autofac bytecode provider implementation.
    /// </summary>
    public class AutofacBytecodeProvider : IBytecodeProvider
    {
        private readonly IComponentContext _container;
        private readonly IProxyFactoryFactory _proxyFactoryFactory;
        private readonly ICollectionTypeFactory _collectionTypeFactory;
        private readonly AutofacObjectsFactory _objectsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacBytecodeProvider"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="proxyFactoryFactory">The proxy factory factory.</param>
        /// <param name="collectionTypeFactory">The collection type factory.</param>
        public AutofacBytecodeProvider(IComponentContext container, IProxyFactoryFactory proxyFactoryFactory, ICollectionTypeFactory collectionTypeFactory)
        {
            _container = container;
            _proxyFactoryFactory = proxyFactoryFactory;
            _collectionTypeFactory = collectionTypeFactory;
            _objectsFactory = new AutofacObjectsFactory(container);
        }

        /// <summary>
        /// Retrieve the <see cref="IReflectionOptimizer" /> delegate for this provider
        /// capable of generating reflection optimization components.
        /// </summary>
        /// <param name="clazz">The class to be reflected upon.</param>
        /// <param name="getters">All property getters to be accessed via reflection.</param>
        /// <param name="setters">All property setters to be accessed via reflection.</param>
        /// <returns>
        /// The reflection optimization delegate.
        /// </returns>
        public IReflectionOptimizer GetReflectionOptimizer(Type clazz, IGetter[] getters, ISetter[] setters)
        {
            return new AutofacReflectionOptimizer(_container, clazz, getters, setters);
        }

        /// <summary>
        /// NHibernate's object instaciator.
        /// </summary>
        /// <remarks>
        /// For entities <see cref="IReflectionOptimizer" /> and its implementations.
        /// </remarks>
        public IObjectsFactory ObjectsFactory
        {
            get { return _objectsFactory; }
        }

        /// <summary>
        /// The specific factory for this provider capable of
        /// generating run-time proxies for lazy-loading purposes.
        /// </summary>
        public IProxyFactoryFactory ProxyFactoryFactory
        {
            get { return _proxyFactoryFactory; }
        }

        /// <summary>
        /// Instanciator of NHibernate's collections default types.
        /// </summary>
        public ICollectionTypeFactory CollectionTypeFactory
        {
            get { return _collectionTypeFactory; }
        }
    }
}