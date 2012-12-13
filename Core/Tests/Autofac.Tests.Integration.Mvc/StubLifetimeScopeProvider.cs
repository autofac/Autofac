using System;
using Autofac.Integration.Mvc;

namespace Autofac.Tests.Integration.Mvc
{
    public class StubLifetimeScopeProvider : ILifetimeScopeProvider
    {
        ILifetimeScope _lifetimeScope;
        readonly ILifetimeScope _container;

        public StubLifetimeScopeProvider(ILifetimeScope container)
        {
            _container = container;
        }

        public ILifetimeScope ApplicationContainer
        {
            get { return _container; }
        }

        public ILifetimeScope GetLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return _lifetimeScope ?? (_lifetimeScope = BuildLifetimeScope(configurationAction));
        }

        public void EndLifetimeScope()
        {
            if (_lifetimeScope != null)
                _lifetimeScope.Dispose();
        }

        ILifetimeScope BuildLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return (configurationAction == null)
                       ? _container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag)
                       : _container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag, configurationAction);
        }
    }
}