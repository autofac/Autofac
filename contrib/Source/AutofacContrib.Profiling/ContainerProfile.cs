using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;

namespace AutofacContrib.Profiling
{
    class ContainerProfile : IContainerProfile
    {
        readonly object _synchRoot = new object();
        readonly IDictionary<Guid, ComponentRegistrationInfo> _componentRegistrationInfo =
            new Dictionary<Guid, ComponentRegistrationInfo>();

        public void RecordActivation(IComponentRegistration component)
        {
            lock (_synchRoot)
            {
                ComponentRegistrationInfo componentInfo = GetComponentInfo(component);
                componentInfo.RecordActivation();
            }
        }

        ComponentRegistrationInfo GetComponentInfo(IComponentRegistration component)
        {
            ComponentRegistrationInfo componentInfo;
            if (!_componentRegistrationInfo.TryGetValue(component.Id, out componentInfo))
            {
                componentInfo = new ComponentRegistrationInfo(component);
                _componentRegistrationInfo.Add(component.Id, componentInfo);
            }
            return componentInfo;
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
                lock (_synchRoot)
                    return _componentRegistrationInfo.Select(cri => cri.Value).ToArray();
            }
        }

        public void RecordDependency(IComponentRegistration from, IComponentRegistration to)
        {
            GetComponentInfo(from).RecordDependency(to.Id);
        }
    }
}
