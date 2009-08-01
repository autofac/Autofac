using System;
using NHibernate.Bytecode;

namespace AutofacContrib.NHibernate
{
	public class AutofacObjectsFactory : IObjectsFactory
	{
		private readonly IContainer container;

		public AutofacObjectsFactory(IContainer container)
		{
			this.container = container;
		}

		public object CreateInstance(Type type)
		{
			return container.IsRegistered(type) ? container.Resolve(type) : Activator.CreateInstance(type);
		}

		public object CreateInstance(Type type, bool nonPublic)
		{
			return container.IsRegistered(type) ? container.Resolve(type) : Activator.CreateInstance(type, nonPublic);
		}

		public object CreateInstance(Type type, params object[] ctorArgs)
		{
			return Activator.CreateInstance(type, ctorArgs);
		}
	}
}