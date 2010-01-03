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
        Type _proxyType;
        IConstructorFinder _decoratedFinder;

        public const string InterceptorsParameterName =
            "AutofacContrib.DynamicProxy2.ProxyConstructorInvoker.InterceptorsParameterName";

        public ProxyConstructorFinder(IConstructorFinder decoratedFinder, Type proxyType)
        {
            if (proxyType == null)
                throw new ArgumentNullException("proxyType");

            if (decoratedFinder == null)
                throw new ArgumentNullException("decoratedFinder");

            _proxyType = proxyType;
            _decoratedFinder = decoratedFinder;
        }

        public object InvokeConstructor(IComponentContext context, IEnumerable<Parameter> parameters, ConstructorInfo ci, Func<object>[] args)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (ci == null)
                throw new ArgumentNullException("ci");
            if (args == null)
                throw new ArgumentNullException("args");

            NamedParameter interceptorsParameter = parameters
                .OfType<NamedParameter>()
                .Where(np => np.Name == InterceptorsParameterName)
                .FirstOrDefault();

            if (interceptorsParameter == null)
                throw new InvalidOperationException();

            var interceptors = (IInterceptor[])interceptorsParameter.Value;

            var argValues = args.Select(a => a());

            var realConstructor = _proxyType.GetConstructor(new[] { typeof(IInterceptor[]) }.Concat(ci.GetParameters().Select(p => p.ParameterType)).ToArray());
            return realConstructor.Invoke(new object[] { interceptors }.Concat(argValues).ToArray());
        }

        public IEnumerable<ConstructorInfo> FindConstructors(Type targetType)
        {
            throw new NotImplementedException();
        }
    }
}
