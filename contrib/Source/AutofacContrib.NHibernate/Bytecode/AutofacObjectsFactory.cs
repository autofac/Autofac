using System;
using System.Linq;
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
            return _container.ResolveOptional(type) ?? Activator.CreateInstance(type);
        }

        public object CreateInstance(Type type, bool nonPublic)
        {
            return _container.ResolveOptional(type) ?? Activator.CreateInstance(type, nonPublic);
        }

        public object CreateInstance(Type type, params object[] ctorArgs)
        {
            return _container.ResolveOptional(
                    type,
                    (ctorArgs ?? Enumerable.Empty<object>()).Select((p, i) => new PositionalParameter(i, p))) ??
                Activator.CreateInstance(type, ctorArgs);
        }
    }
}