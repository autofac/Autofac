using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Diagnostics;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Middleware;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Test
{
    internal static class Mocks
    {
        public static IConstructorFinder GetConstructorFinder()
        {
            return new MockConstructorFinder();
        }

        public static IConstructorSelector GetConstructorSelector()
        {
            return new MockConstructorSelector();
        }

        public static MockComponentRegistration GetComponentRegistration()
        {
            return new MockComponentRegistration();
        }

        public static ServiceRegistration GetResolvableImplementation()
        {
            return new ServiceRegistration(
                ServicePipelines.DefaultServicePipeline,
                GetComponentRegistration());
        }

        public static ServiceRegistration GetResolvableImplementation(IComponentRegistration registration)
        {
            return new ServiceRegistration(
                ServicePipelines.DefaultServicePipeline,
                registration);
        }

        public static MockTracer GetTracer()
        {
            return new MockTracer();
        }

        internal class MockConstructorFinder : IConstructorFinder
        {
            public ConstructorInfo[] FindConstructors(Type targetType)
            {
                return new ConstructorInfo[0];
            }
        }

        internal class MockConstructorSelector : IConstructorSelector
        {
            public ConstructorParameterBinding SelectConstructorBinding(ConstructorParameterBinding[] constructorBindings, IEnumerable<Parameter> parameters)
            {
                return null;
            }
        }

        internal class MockComponentRegistration : IComponentRegistration
        {
            public void Dispose()
            {
                IsDisposed = true;
            }

            public bool IsDisposed { get; private set; }

            public Guid Id { get; }

            public IInstanceActivator Activator { get; }

            public IComponentLifetime Lifetime { get; }

            public InstanceSharing Sharing { get; }

            public InstanceOwnership Ownership { get; }

            public IEnumerable<Service> Services { get; } = new Service[0];

            public IDictionary<string, object> Metadata { get; }

            public IComponentRegistration Target { get; }

            public bool IsAdapterForIndividualComponent { get; }

            public event EventHandler<IResolvePipelineBuilder> PipelineBuilding;

            public IResolvePipeline ResolvePipeline { get; } = new ResolvePipelineBuilder(PipelineType.Registration).Build();

            public bool IsServiceOverride { get; set; }

            public void BuildResolvePipeline(IComponentRegistryServices registryServices)
            {
                PipelineBuilding?.Invoke(this, new ResolvePipelineBuilder(PipelineType.Registration));
            }
        }

        internal class MockTracer : IResolvePipelineTracer
        {
            public MockTracer()
            {
            }

            public event Action<ResolveOperationBase, ResolveRequest> OperationStarting;

            public event Action<ResolveOperationBase, ResolveRequestContextBase> RequestStarting;

            public event Action<ResolveOperationBase, ResolveRequestContextBase, IResolveMiddleware> EnteringMiddleware;

            public event Action<ResolveOperationBase, ResolveRequestContextBase, IResolveMiddleware, bool> ExitingMiddleware;

            public event Action<ResolveOperationBase, ResolveRequestContextBase, Exception> RequestFailing;

            public event Action<ResolveOperationBase, ResolveRequestContextBase> RequestSucceeding;

            public event Action<ResolveOperationBase, Exception> OperationFailing;

            public event Action<ResolveOperationBase, object> OperationSucceeding;

            public void OperationStart(ResolveOperationBase operation, ResolveRequest initiatingRequest)
            {
                OperationStarting?.Invoke(operation, initiatingRequest);
            }

            public void RequestStart(ResolveOperationBase operation, ResolveRequestContextBase requestContext)
            {
                RequestStarting?.Invoke(operation, requestContext);
            }

            public void MiddlewareEntry(ResolveOperationBase operation, ResolveRequestContextBase requestContext, IResolveMiddleware middleware)
            {
                EnteringMiddleware?.Invoke(operation, requestContext, middleware);
            }

            public void MiddlewareExit(ResolveOperationBase operation, ResolveRequestContextBase requestContext, IResolveMiddleware middleware, bool succeeded)
            {
                ExitingMiddleware?.Invoke(operation, requestContext, middleware, succeeded);
            }

            public void RequestFailure(ResolveOperationBase operation, ResolveRequestContextBase requestContext, Exception requestException)
            {
                RequestFailing?.Invoke(operation, requestContext, requestException);
            }

            public void RequestSuccess(ResolveOperationBase operation, ResolveRequestContextBase requestContext)
            {
                RequestSucceeding?.Invoke(operation, requestContext);
            }

            public void OperationFailure(ResolveOperationBase operation, Exception operationException)
            {
                OperationFailing?.Invoke(operation, operationException);
            }

            public void OperationSuccess(ResolveOperationBase operation, object resolvedInstance)
            {
                OperationSucceeding?.Invoke(operation, resolvedInstance);
            }
        }
    }
}
