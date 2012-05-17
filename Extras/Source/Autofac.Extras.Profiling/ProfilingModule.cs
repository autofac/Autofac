using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac.Extras.Profiling
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

            _profile.AddComponent(registration);
            standardRegistration.Activator = new ProfilingActivator(
                registration,
                standardRegistration.Activator,
                _profile);
        }
    }
}
