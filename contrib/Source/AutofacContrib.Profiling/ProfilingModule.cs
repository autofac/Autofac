using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;

namespace AutofacContrib.Profiling
{
    public class ProfilingModule : Module
    {
        readonly ContainerProfile _profile = new ContainerProfile();

        protected override void Load(ContainerBuilder moduleBuilder)
        {
            base.Load(moduleBuilder);
            moduleBuilder.RegisterInstance(_profile).As<IContainerProfile>();
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            base.AttachToComponentRegistration(componentRegistry, registration);

            var standardRegistration = registration as ComponentRegistration;
            if (standardRegistration == null)
                return;

            standardRegistration.Activator = new ProfilingActivator(
                registration,
                standardRegistration.Activator,
                _profile);
        }
    }

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

    class DependencyTrackingContext : IComponentContext
    {
        readonly ContainerProfile _profile;
        readonly IComponentRegistration _registration;
        readonly IComponentContext _context;

        public DependencyTrackingContext(
            ContainerProfile profile,
            IComponentRegistration registration,
            IComponentContext context)
        {
            _profile = profile;
            _registration = registration;
            _context = context;
        }

        public object Resolve(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            _profile.RecordDependency(_registration, registration);
            return _context.Resolve(registration, parameters);
        }

        public IComponentRegistry ComponentRegistry
        {
            get { return _context.ComponentRegistry; }
        }
    }
}
