using System;
using Autofac.Core;
using NHibernate.Bytecode.Lightweight;
using NHibernate.Properties;
using Autofac;

namespace AutofacContrib.NHibernate.Bytecode
{
    public class AutofacReflectionOptimizer : ReflectionOptimizer
    {
        private readonly IComponentContext _container;

        public AutofacReflectionOptimizer(IComponentContext container, Type mappedType, IGetter[] getters, ISetter[] setters)
            : base(mappedType, getters, setters)
        {
            _container = container;
        }

        public override object CreateInstance()
        {
            if (_container.IsRegistered(mappedType))
                return _container.Resolve(mappedType);
            
            return _container.IsRegisteredWithName(mappedType.FullName, mappedType)
                       ? _container.ResolveNamed(mappedType.FullName, mappedType)
                       : base.CreateInstance();
        }

        protected override void ThrowExceptionForNoDefaultCtor(Type type) { }
    }
}