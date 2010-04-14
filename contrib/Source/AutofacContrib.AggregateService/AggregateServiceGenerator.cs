using System;
using Autofac;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;

namespace AutofacContrib.AggregateService
{
    public static class AggregateServiceGenerator
    {
        private static readonly ProxyGenerator Generator;

        static AggregateServiceGenerator()
        {
            Generator = new ProxyGenerator();
        }

        public static object CreateInstance<TAggregateServiceInterface>(IComponentContext context)
        {
            return CreateInstance(typeof (TAggregateServiceInterface), context);
        }

        public static object CreateInstance(Type interfaceType, IComponentContext context)
        {
            var resolverInterceptor = new ResolvingInterceptor(context);
            return Generator.CreateInterfaceProxyWithoutTarget(interfaceType, resolverInterceptor);
        }
    }
}