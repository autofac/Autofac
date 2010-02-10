using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;

namespace AutofacContrib.Profiling
{
    class ContainerProfile : IContainerProfile
    {
        readonly object _synchRoot = new object();
        readonly IDictionary<IComponentRegistration, int> _activationCounts = 
            new Dictionary<IComponentRegistration, int>();

        public void RecordActivation(
            IComponentRegistration component, 
            IEnumerable<Parameter> parameters)
        {
            lock (_synchRoot)
            {
                int count;
                if (!_activationCounts.TryGetValue(component, out count))
                    count = 0;
                _activationCounts[component] = count + 1;
            }
        }

        public IEnumerable<KeyValuePair<IComponentRegistration, int>> ActivationCounts
        {
            get
            {
                lock (_synchRoot)
                    return _activationCounts.ToArray();
            }
        }
    }
}
