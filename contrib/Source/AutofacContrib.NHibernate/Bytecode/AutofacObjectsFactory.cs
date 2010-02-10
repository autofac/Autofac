using System;
using Autofac.Core;
using NHibernate.Bytecode;
using Autofac;

namespace AutofacContrib.NHibernate.Bytecode
{
    public class AutofacObjectsFactory : IObjectsFactory
    {
        private readonly IComponentContext _container;

        public AutofacObjectsFactory(IComponentContext container)
        {
            _container = container;
        }

        public object CreateInstance(Type type)
        {
            return _container.IsRegistered(type) ? _container.Resolve(type) : Activator.CreateInstance(type);
        }

        public object CreateInstance(Type type, bool nonPublic)
        {
            return _container.IsRegistered(type) ? _container.Resolve(type) : Activator.CreateInstance(type, nonPublic);
        }

        public object CreateInstance(Type type, params object[] ctorArgs)
        {
            return Activator.CreateInstance(type, ctorArgs);
        }
    }
}