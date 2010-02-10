using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Castle.Core.Interceptor;
using System.Collections.Generic;

namespace AutofacContrib.DynamicProxy2
{
    class ProxyConstructorFinder : IConstructorFinder
    {
        readonly Type _proxyType;
        readonly IConstructorFinder _decoratedFinder;

        public ProxyConstructorFinder(IConstructorFinder decoratedFinder, Type proxyType)
        {
            if (proxyType == null)
                throw new ArgumentNullException("proxyType");

            if (decoratedFinder == null)
                throw new ArgumentNullException("decoratedFinder");

            _proxyType = proxyType;
            _decoratedFinder = decoratedFinder;
        }

        public IEnumerable<ConstructorInfo> FindConstructors(Type targetType)
        {
            var targetCtors = _decoratedFinder.FindConstructors(targetType);
            return targetCtors
                .Select(t => _proxyType.GetConstructor(
                    new[] { typeof(IInterceptor[]) }
                    .Concat(t.GetParameters().Select(p => p.ParameterType)).ToArray()));
        }
    }
}
