using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;

namespace Autofac.Extras.Profiling
{
    public class ContainerProfile : IContainerProfile
    {
        readonly object _synchRoot = new object();
        readonly IDictionary<Guid, ComponentRegistrationInfo> _componentRegistrationInfo =
            new Dictionary<Guid, ComponentRegistrationInfo>();

        public void RecordActivation(IComponentRegistration component)
        {
            lock (_synchRoot)
            {
                var componentInfo = GetComponent(component.Id);
                componentInfo.RecordActivation();
            }
        }

        public ComponentRegistrationInfo GetComponent(Guid id)
        {
            lock (_synchRoot)
                return _componentRegistrationInfo[id];
        }

        public IEnumerable<ComponentRegistrationInfo> Components
        {
            get
            {
                // Really should deep copy here to avoid concurrency issues,
                // however it is no big deal under the debugger.
                lock (_synchRoot)
                    return _componentRegistrationInfo.Select(cri => cri.Value).ToArray();
            }
        }

        public void RecordDependency(IComponentRegistration from, IComponentRegistration to)
        {
            lock (_synchRoot)
                GetComponent(from.Id).RecordDependency(to.Id);
        }

        public void AddComponent(IComponentRegistration registration)
        {
            lock (_synchRoot)
                _componentRegistrationInfo.Add(registration.Id, new ComponentRegistrationInfo(registration));
        }
    }
}
