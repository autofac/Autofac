using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Castle.Core.Interceptor;

namespace AutofacContrib.AggregateService
{
    /// <summary>
    /// Interceptor that resolves types of properties and methods using a <see cref="IComponentContext"/>.
    /// </summary>
    public class ResolvingInterceptor : IInterceptor
    {
        private readonly IComponentContext _context;
        private readonly Dictionary<MethodInfo, object> _services = new Dictionary<MethodInfo, object>();
        private readonly Dictionary<MethodInfo, Action<IInvocation>> _methodMap;

        public ResolvingInterceptor(Type interfaceType, IComponentContext context)
        {
            _context = context;
            _methodMap = SetupInvocationMap(interfaceType);
        }

        public void Intercept(IInvocation invocation)
        {
            var invocationHandler = _methodMap[invocation.Method];
            invocationHandler(invocation);
        }

        private Dictionary<MethodInfo, Action<IInvocation>> SetupInvocationMap(Type interfaceType)
        {
            var methods = interfaceType.GetMethods();
            var methodMap = new Dictionary<MethodInfo, Action<IInvocation>>(methods.Length);
            foreach (var method in methods)
            {
                if (method.ReturnType == typeof(void))
                {
                    methodMap.Add(method, InvalidReturnTypeInvocation);
                }
                else if (GetProperty(method) != null)
                {
                    methodMap.Add(method, PropertyInvocation);
                }
                else
                {
                    var returnType = method.ReturnType;
                    var parameters = method.GetParameters()
                        .OrderBy(parameterInfo => parameterInfo.Position)
                        .Select(parameterInfo => new { parameterInfo.Position, parameterInfo.ParameterType })
                        .ToArray();

                    if (parameters.Length > 0)
                    {
                        methodMap.Add(method, invocation =>
                                                  {
                                                      var arguments = invocation.Arguments;
                                                      var typedParameters = parameters
                                                          .Select(info => (Parameter)new TypedParameter(info.ParameterType, arguments[info.Position]));

                                                      invocation.ReturnValue = _context.Resolve(returnType, typedParameters);
                                                  });
                    }
                    else
                    {
                        methodMap.Add(method, invocation => invocation.ReturnValue = _context.Resolve(returnType));
                    }
                }
            }

            return methodMap;
        }

        private void PropertyInvocation(IInvocation invocation)
        {
            object service;
            if (!_services.TryGetValue(invocation.Method, out service))
            {
                var returnType = invocation.Method.ReturnType;
                service = _context.Resolve(returnType);
                _services.Add(invocation.Method, service);
            }

            invocation.ReturnValue = service;
        }

        private static void InvalidReturnTypeInvocation(IInvocation invocation)
        {
            throw new InvalidOperationException("The method " + invocation.Method + " has invalid return type System.Void");
        }

        private static PropertyInfo GetProperty(MethodInfo method)
        {
            var takesArg = method.GetParameters().Length == 1;
            var hasReturn = method.ReturnType != typeof(void);
            if (takesArg == hasReturn) return null;
            // Ignore setters
            //if (takesArg)
            //{
            //    return method.DeclaringType.GetProperties()
            //        .Where(prop => prop.GetSetMethod() == method).FirstOrDefault();
            //}

            return method.DeclaringType.GetProperties()
                .Where(prop => prop.GetGetMethod() == method).FirstOrDefault();
        }
    }
}