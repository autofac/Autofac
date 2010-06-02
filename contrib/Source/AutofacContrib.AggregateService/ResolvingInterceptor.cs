// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2010 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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
        private readonly Dictionary<MethodInfo, Action<IInvocation>> _invocationMap;

        ///<summary>
        /// Initialize <see cref="ResolvingInterceptor"/> with an interface type and a component context.
        ///</summary>
        public ResolvingInterceptor(Type interfaceType, IComponentContext context)
        {
            _context = context;
            _invocationMap = SetupInvocationMap(interfaceType);
        }

        void IInterceptor.Intercept(IInvocation invocation)
        {
            var invocationHandler = _invocationMap[invocation.Method];
            invocationHandler(invocation);
        }

        private Dictionary<MethodInfo, Action<IInvocation>> SetupInvocationMap(Type interfaceType)
        {
            var methods = interfaceType.GetMethods();
            var methodMap = new Dictionary<MethodInfo, Action<IInvocation>>(methods.Length);
            foreach (var method in methods)
            {
                var returnType = method.ReturnType;

                if (returnType == typeof(void))
                {
                    // Any method with 'void' return type (includes property setters) should throw an exception
                    methodMap.Add(method, InvalidReturnTypeInvocation);
                }
                else if (GetProperty(method) != null)
                {
                    // All properties should be resolved at proxy instantiation 
                    var propertyValue = _context.Resolve(returnType);
                    methodMap.Add(method, invocation => invocation.ReturnValue = propertyValue);
                }
                else
                {
                    // For methods with parameters, cache parameter info for use at invocation time
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
                        var methodWithoutParams = GetType()
                            .GetMethod("MethodWithoutParams", BindingFlags.Instance|BindingFlags.NonPublic)
                            .MakeGenericMethod(new[]{ returnType });

                        var methodWithoutParamsDelegate = (Action<IInvocation>)Delegate.CreateDelegate(typeof (Action<IInvocation>), this, methodWithoutParams);
                        methodMap.Add(method, methodWithoutParamsDelegate);
                    }
                }
            }

            return methodMap;
        }

        private void MethodWithoutParams<TReturnType>(IInvocation invocation)
        {
            invocation.ReturnValue = _context.Resolve<TReturnType>();
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