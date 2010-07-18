using System;
using System.Collections.Generic;
using Autofac.Core;

namespace AutofacContrib.Profiling
{
    public class ComponentRegistrationInfo
    {
        readonly IComponentRegistration _componentRegistration;
        int _activationCount;
        readonly ICollection<Guid> _dependencies = new HashSet<Guid>();

        public ComponentRegistrationInfo(IComponentRegistration componentRegistration)
        {
            _componentRegistration = componentRegistration;
        }

        public IComponentRegistration ComponentRegistration
        {
            get { return _componentRegistration; }
        }

        public int ActivationCount
        {
            get { return _activationCount; }
        }

        public void RecordActivation()
        {
            ++_activationCount;
        }

        public void RecordDependency(Guid dependentComponentId)
        {
            _dependencies.Add(dependentComponentId);
        }

        public IEnumerable<Guid> Dependencies
        {
            get { return _dependencies; }
        }
    }
}
