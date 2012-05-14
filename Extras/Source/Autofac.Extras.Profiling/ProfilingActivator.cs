using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace AutofacContrib.Profiling
{
    public class ProfilingActivator : IInstanceActivator
    {
        readonly IComponentRegistration _registration;
        readonly IInstanceActivator _innerActivator;
        readonly ContainerProfile _profile;

        public ProfilingActivator(
            IComponentRegistration registration,
            IInstanceActivator innerActivator,
            ContainerProfile profile)
        {
            _registration = registration;
            _innerActivator = innerActivator;
            _profile = profile;
        }

        public void Dispose()
        {
            InnerActivator.Dispose();
        }

        public object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            _profile.RecordActivation(_registration);

            return InnerActivator.ActivateInstance(
                new DependencyTrackingContext(_profile, _registration, context),
                parameters);
        }

        public Type LimitType
        {
            get { return InnerActivator.LimitType; }
        }

        public IInstanceActivator InnerActivator
        {
            get { return _innerActivator; }
        }

        public override string ToString()
        {
            return InnerActivator + " [Profiled]";
        }
    }
}