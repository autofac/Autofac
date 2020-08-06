using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Diagnostics;

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
            public BoundConstructor SelectConstructorBinding(BoundConstructor[] constructorBindings, IEnumerable<Parameter> parameters)
            {
                return default;
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

            public RegistrationOptions Options { get; set; }

            public void BuildResolvePipeline(IComponentRegistryServices registryServices)
            {
                PipelineBuilding?.Invoke(this, new ResolvePipelineBuilder(PipelineType.Registration));
            }
        }

        internal class MockTracer : DiagnosticTracerBase
        {
            public MockTracer()
            {
                this.EnableAll();
            }

            public event Action<IResolveOperation, ResolveRequest> OperationStarting;

            public event Action<IResolveOperation, ResolveRequestContext> RequestStarting;

            public event Action<ResolveRequestContext, IResolveMiddleware> EnteringMiddleware;

            public event Action<ResolveRequestContext, IResolveMiddleware, bool> ExitingMiddleware;

            public event Action<IResolveOperation, ResolveRequestContext, Exception> RequestFailing;

            public event Action<IResolveOperation, ResolveRequestContext> RequestSucceeding;

            public event Action<IResolveOperation, Exception> OperationFailing;

            public event Action<IResolveOperation, object> OperationSucceeding;

            protected override void OnOperationStart(OperationStartDiagnosticData data)
            {
                OperationStarting?.Invoke(data.Operation, data.InitiatingRequest);
            }

            protected override void OnRequestStart(RequestDiagnosticData data)
            {
                RequestStarting?.Invoke(data.Operation, data.RequestContext);
            }

            protected override void OnMiddlewareStart(MiddlewareDiagnosticData data)
            {
                EnteringMiddleware?.Invoke(data.RequestContext, data.Middleware);
            }

            protected override void OnMiddlewareFailure(MiddlewareDiagnosticData data)
            {
                ExitingMiddleware?.Invoke(data.RequestContext, data.Middleware, false);
            }

            protected override void OnMiddlewareSuccess(MiddlewareDiagnosticData data)
            {
                ExitingMiddleware?.Invoke(data.RequestContext, data.Middleware, true);
            }

            protected override void OnRequestFailure(RequestFailureDiagnosticData data)
            {
                RequestFailing?.Invoke(data.Operation, data.RequestContext, data.RequestException);
            }

            protected override void OnRequestSuccess(RequestDiagnosticData data)
            {
                RequestSucceeding?.Invoke(data.Operation, data.RequestContext);
            }

            protected override void OnOperationFailure(OperationFailureDiagnosticData data)
            {
                OperationFailing?.Invoke(data.Operation, data.OperationException);
            }

            protected override void OnOperationSuccess(OperationSuccessDiagnosticData data)
            {
                OperationSucceeding?.Invoke(data.Operation, data.ResolvedInstance);
            }
        }
    }
}
