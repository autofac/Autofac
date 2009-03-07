using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Component.Activation;
using Castle.Core.Interceptor;
using System.Collections.Generic;

namespace AutofacContrib.DynamicProxy2
{
    class ProxyConstructorInvoker : IConstructorInvoker
    {
        Type _proxyType;

        public const string InterceptorsParameterName = "AutofacContrib.DynamicProxy2.ProxyConstructorInvoker.InterceptorsParameterName";

        public ProxyConstructorInvoker(Type proxyType)
        {
            if (proxyType == null)
                throw new ArgumentNullException("proxyType");

            _proxyType = proxyType;
        }

        public object InvokeConstructor(IContext context, IEnumerable<Parameter> parameters, ConstructorInfo ci, Func<object>[] args)
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
    }
}
