using System;
using System.Collections.Generic;
using Autofac.Util;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Wraps a component registration, switching its lifetime.
    /// </summary>
    class ComponentRegistrationLifetimeDecorator : Disposable, IComponentRegistration
    {
        readonly IComponentLifetime _lifetime;
        readonly IComponentRegistration _inner;

        public ComponentRegistrationLifetimeDecorator(IComponentRegistration inner, IComponentLifetime lifetime)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (lifetime == null) throw new ArgumentNullException("lifetime");

            _inner = inner;
            _lifetime = lifetime;
        }

        public Guid Id
        {
            get { return _inner.Id; }
        }

        public IInstanceActivator Activator
        {
            get { return _inner.Activator; }
        }

        public IComponentLifetime Lifetime
        {
            get { return _lifetime; }
        }

        public InstanceSharing Sharing
        {
            get { return _inner.Sharing; }
        }

        public InstanceOwnership Ownership
        {
            get { return _inner.Ownership; }
        }

        public IEnumerable<Service> Services
        {
            get { return _inner.Services; }
        }

        public IDictionary<string, object> Metadata
        {
            get { return _inner.Metadata; }
        }

        public IComponentRegistration Target
        {
            get
            {
                if (_inner.IsAdapting())
                    return _inner.Target;

                return this;
            }
        }

        public event EventHandler<PreparingEventArgs> Preparing
        {
            add { _inner.Preparing += value; }
            remove { _inner.Preparing -= value; }
        }

        public void RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters)
        {
            _inner.RaisePreparing(context, ref parameters);
        }

        public event EventHandler<ActivatingEventArgs<object>> Activating
        {
            add { _inner.Activating += value; }
            remove { _inner.Activating -= value; }
        }

        public void RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance)
        {
            _inner.RaiseActivating(context, parameters, ref instance);
        }

        public event EventHandler<ActivatedEventArgs<object>> Activated
        {
            add { _inner.Activated += value; }
            remove { _inner.Activated -= value; }
        }

        public void RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance)
        {
            _inner.RaiseActivated(context, parameters, instance);
        }
    }
}
