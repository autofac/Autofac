using System;
using Autofac.Integration.Mvc;

namespace Autofac.Tests.Integration.Mvc
{
    public class StubLifetimeScopeProvider : ILifetimeScopeProvider
    {
        ILifetimeScope _lifetimeScope;
        readonly ILifetimeScope _container;
        readonly Action<ContainerBuilder> _configurationAction;

        public StubLifetimeScopeProvider(ILifetimeScope container)
        {
            _container = container;
        }

        public StubLifetimeScopeProvider(ILifetimeScope container, Action<ContainerBuilder> configurationAction)
        {
            _container = container;
            _configurationAction = configurationAction;
        }

        public ILifetimeScope ApplicationContainer
        {
            get { return _container; }
        }

        public ILifetimeScope GetLifetimeScope()
        {
            return _lifetimeScope ?? (_lifetimeScope = BuildLifetimeScope());
        }

        public void EndLifetimeScope()
        {
            if (_lifetimeScope != null)
                _lifetimeScope.Dispose();
        }

        ILifetimeScope BuildLifetimeScope()
        {
            return (_configurationAction == null)
                       ? _container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag)
                       : _container.BeginLifetimeScope(RequestLifetimeScopeProvider.HttpRequestTag, _configurationAction);
        }
    }
}