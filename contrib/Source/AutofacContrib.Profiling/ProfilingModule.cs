using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace AutofacContrib.Profiling
{
    public class ProfilingModule : Module
    {
        ContainerProfile _profile = new ContainerProfile();

        protected override void Load(ContainerBuilder moduleBuilder)
        {
            base.Load(moduleBuilder);
            moduleBuilder.RegisterInstance(_profile).As<IContainerProfile>();
        }

        protected override void AttachToComponentRegistration(Autofac.Core.IComponentRegistry componentRegistry, Autofac.Core.IComponentRegistration registration)
        {
            base.AttachToComponentRegistration(componentRegistry, registration);
            registration.Activated += (s, e) =>
            {
                _profile.RecordActivation(e.Component, e.Parameters);
            };
        }
    }
}
