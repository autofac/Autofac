using System;
using Autofac;
using Autofac.Core;
using NHibernate.Bytecode;
using NHibernate.Properties;

namespace AutofacContrib.NHibernate.Bytecode
{
    public class AutofacBytecodeProvider : IBytecodeProvider
    {
        private readonly IComponentContext _container;
        private readonly IProxyFactoryFactory _proxyFactoryFactory;
        private readonly ICollectionTypeFactory _collectionTypeFactory;
        private readonly AutofacObjectsFactory _objectsFactory;

        public AutofacBytecodeProvider(IComponentContext container, IProxyFactoryFactory proxyFactoryFactory, ICollectionTypeFactory collectionTypeFactory)
        {
            _container = container;
            _proxyFactoryFactory = proxyFactoryFactory;
            _collectionTypeFactory = collectionTypeFactory;
            _objectsFactory = new AutofacObjectsFactory(container);
        }

        public IReflectionOptimizer GetReflectionOptimizer(Type clazz, IGetter[] getters, ISetter[] setters)
        {
            return new AutofacReflectionOptimizer(_container, clazz, getters, setters);
        }

        public IObjectsFactory ObjectsFactory
        {
            get { return _objectsFactory; }
        }

        public IProxyFactoryFactory ProxyFactoryFactory
        {
            get { return _proxyFactoryFactory; }
        }

        public ICollectionTypeFactory CollectionTypeFactory
        {
            get { return _collectionTypeFactory; }
        }
    }
}