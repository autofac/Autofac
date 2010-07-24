using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Get the dependencies of the component, if they are known.
        /// </summary>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public bool TryGetDependencies(out IEnumerable<Guid> dependencies)
        {
            if (ActivationCount == 0)
            {
                dependencies = null;
                return false;
            }

            dependencies = _dependencies.ToArray();
            return true;
        }
    }
}
