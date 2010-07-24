using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace AutofacContrib.Profiling
{
    class ProfilingActivator : IInstanceActivator
    {
        readonly IComponentRegistration _registration;
        readonly IInstanceActivator _originalActivator;
        readonly ContainerProfile _profile;

        public ProfilingActivator(
            IComponentRegistration registration,
            IInstanceActivator originalActivator,
            ContainerProfile profile)
        {
            _registration = registration;
            _originalActivator = originalActivator;
            _profile = profile;
        }

        public void Dispose()
        {
            _originalActivator.Dispose();
        }

        public object ActivateInstance(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            _profile.RecordActivation(_registration);

            return _originalActivator.ActivateInstance(
                new DependencyTrackingContext(_profile, _registration, context),
                parameters);
        }

        public Type LimitType
        {
            get { return _originalActivator.LimitType; }
        }
    }
}