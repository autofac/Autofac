using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Practices.ServiceLocation;

namespace AutofacContrib.CommonServiceLocator
{
	public sealed class AutofacServiceLocator : ServiceLocatorImplBase
	{
		readonly IContext _container;

		public AutofacServiceLocator(IContext container)
		{
			if (container == null)
				throw new ArgumentNullException("container");
			_container = container;
		}

		protected override object DoGetInstance(Type serviceType, string key)
		{
			if (key != null)
				return _container.Resolve(key);
			return _container.Resolve(serviceType);
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
		{
			var type = typeof (IEnumerable<>).MakeGenericType(serviceType);

			object instance;
			if (_container.TryResolve(type, out instance))
			{
				return ((IEnumerable) instance).Cast<object>();
			}

			return Enumerable.Empty<object>();
		}
	}
}