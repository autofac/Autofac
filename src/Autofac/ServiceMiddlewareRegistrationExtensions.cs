using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using System;

namespace Autofac
{
    public static class ServiceMiddlewareRegistrationExtensions
    {
        public static void RegisterServiceMiddleware(this ContainerBuilder builder, Service service, IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
        {
            builder.RegisterCallback(crb => crb.RegisterServiceMiddleware(service, middleware, insertionMode));
        }

        public static void RegisterServiceMiddleware<TService>(this ContainerBuilder builder, IResolveMiddleware middleware, MiddlewareInsertionMode insertionMode = MiddlewareInsertionMode.EndOfPhase)
        {
            builder.RegisterServiceMiddleware(new TypedService(typeof(TService)), middleware, insertionMode);
        }

        public static IServiceMiddlewareSourceRegistrar RegisterServicePipelineSource(this ContainerBuilder builder, IServiceMiddlewareSource servicePipelineSource)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));
            if (servicePipelineSource is null) throw new ArgumentNullException(nameof(servicePipelineSource));

            var registrar = new ServicePipelineSourceRegistrar(builder);
            return registrar.RegisterServiceMiddlewareSource(servicePipelineSource);
        }
    }
}
