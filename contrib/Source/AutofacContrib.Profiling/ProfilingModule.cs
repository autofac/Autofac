using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace AutofacContrib.Profiling
{
    public class ProfilingModule : Module
    {
        [ThreadStatic]
        static readonly Stack<IComponentRegistration> ActivationStack = new Stack<IComponentRegistration>();

        readonly ContainerProfile _profile = new ContainerProfile();

        protected override void Load(ContainerBuilder moduleBuilder)
        {
            base.Load(moduleBuilder);
            moduleBuilder.RegisterInstance(_profile).As<IContainerProfile>();
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, Autofac.Core.IComponentRegistration registration)
        {
            base.AttachToComponentRegistration(componentRegistry, registration);

            registration.Activated += (s, e) => _profile.RecordActivation(e.Component);
            
            registration.Preparing += (s, e) =>
            {
                if (ActivationStack.Count != 0)
                    _profile.RecordDependency(ActivationStack.Peek(), e.Component);

                ActivationStack.Push(e.Component);
            };

            registration.Activating += (s, e) => ActivationStack.Pop();
        }
    }
}
