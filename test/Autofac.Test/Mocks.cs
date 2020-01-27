using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

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

            public event EventHandler<PreparingEventArgs> Preparing = (sender, args) => { };

            public void RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters)
            {
            }

            public event EventHandler<ActivatingEventArgs<object>> Activating = (sender, args) => { };

            public void RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance)
            {
            }

            public event EventHandler<ActivatedEventArgs<object>> Activated = (sender, args) => { };

            public void RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance)
            {
            }
        }
    }
}