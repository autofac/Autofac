using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Diagnostics;
using Autofac.Core.Resolving;
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

        public static MockPipelineOperation GetPipelineOperation(ISharingLifetimeScope scope)
        {
            return new MockPipelineOperation(scope);
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

            public IResolvePipeline ResolvePipeline { get; } = new ResolvePipelineBuilder().Build();

            public void BuildResolvePipeline(IComponentRegistryServices registryServices)
            {
            }
        }

        public class MockPipelineOperation : IPipelineResolveOperation
        {
            public MockPipelineOperation(ISharingLifetimeScope scope)
            {
                CurrentScope = scope;
            }

            public ISharingLifetimeScope CurrentScope { get; }

            public IComponentRegistry ComponentRegistry => CurrentScope.ComponentRegistry;

            public IResolveRequestContext ActiveRequestContext => throw new NotImplementedException();

            public IEnumerable<IResolveRequestContext> InProgressRequests => throw new NotImplementedException();

            Stack<IResolveRequestContext> IPipelineResolveOperation.RequestStack => throw new NotImplementedException();

            public int RequestDepth => throw new NotImplementedException();

            public ITracingIdentifer TracingId => this;

            public bool IsTopLevelOperation => true;

            public ResolveRequest InitiatingRequest { get; set; }

            public event EventHandler<ResolveOperationEndingEventArgs> CurrentOperationEnding;

            public event EventHandler<ResolveRequestBeginningEventArgs> ResolveRequestBeginning;

            public object GetOrCreateInstance(ISharingLifetimeScope currentOperationScope, ResolveRequest request)
            {
                return currentOperationScope.ResolveComponent(request);
            }

            public object ResolveComponent(ResolveRequest request)
            {
                return CurrentScope.ResolveComponent(request);
            }
        }
    }
}
