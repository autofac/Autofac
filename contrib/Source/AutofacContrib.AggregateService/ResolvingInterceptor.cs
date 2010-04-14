using System;
using System.Linq;
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

        public ResolvingInterceptor(IComponentContext context)
        {
            _context = context;
        }

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            if (returnType == typeof(void))
                throw new InvalidOperationException("The method " + invocation.Method + " has invalid return type System.Void");

            var arguments = invocation.Arguments;
            if (arguments.Length > 0)
            {
                //TODO: cache method parameter types
                var parameters = invocation.Method.GetParameters()
                    .OrderBy(parameterInfo => parameterInfo.Position)
                    .Select(parameterInfo => new { parameterInfo.Position, parameterInfo.ParameterType})
                    .Select(info => (Parameter)new TypedParameter(info.ParameterType, arguments[info.Position]));

                invocation.ReturnValue = _context.Resolve(returnType, parameters);
            }
            else
            {
                invocation.ReturnValue = _context.Resolve(returnType);
            }
        }
    }
}