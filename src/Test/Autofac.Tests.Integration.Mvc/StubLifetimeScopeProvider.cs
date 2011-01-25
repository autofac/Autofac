using System;
using Autofac.Integration.Mvc;

namespace Autofac.Tests.Integration.Mvc
{
    public class StubLifetimeScopeProvider : ILifetimeScopeProvider
    {
        ILifetimeScope _lifetimeScope;

        public ILifetimeScope GetLifetimeScope(ILifetimeScope container, Action<ContainerBuilder> configurationAction)
        {
            return _lifetimeScope ?? (_lifetimeScope = GetLifetimeScope(configurationAction, container));
        }

        static ILifetimeScope GetLifetimeScope(Action<ContainerBuilder> requestLifetimeConfiguration, ILifetimeScope container)
        {
            return (requestLifetimeConfiguration == null)
                       ? container.BeginLifetimeScope(RequestLifetimeHttpModule.HttpRequestTag)
                       : container.BeginLifetimeScope(RequestLifetimeHttpModule.HttpRequestTag, requestLifetimeConfiguration);
        }
    }
}