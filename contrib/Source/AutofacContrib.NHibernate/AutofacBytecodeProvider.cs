using System;
using NHibernate.Bytecode;
using NHibernate.Properties;
using Autofac;

namespace AutofacContrib.NHibernate
{
	public class AutofacBytecodeProvider : IBytecodeProvider
	{
		private readonly IContainer container;
		private readonly IProxyFactoryFactory proxyFactoryFactory;
		private readonly ICollectionTypeFactory collectionTypeFactory;
		private readonly AutofacObjectsFactory objectsFactory;

		public AutofacBytecodeProvider(IContainer container, IProxyFactoryFactory proxyFactoryFactory, ICollectionTypeFactory collectionTypeFactory)
		{
			this.container = container;
			this.proxyFactoryFactory = proxyFactoryFactory;
			this.collectionTypeFactory = collectionTypeFactory;
			this.objectsFactory = new AutofacObjectsFactory(container);
		}

		public IReflectionOptimizer GetReflectionOptimizer(Type clazz, IGetter[] getters, ISetter[] setters)
		{
			return new AutofacReflectionOptimizer(container, clazz, getters, setters);
		}

		public IObjectsFactory ObjectsFactory
		{
			get { return objectsFactory; }
		}

		public IProxyFactoryFactory ProxyFactoryFactory
		{
			get { return proxyFactoryFactory; }
		}

		public ICollectionTypeFactory CollectionTypeFactory
		{
			get { return collectionTypeFactory; }
		}
	}
}