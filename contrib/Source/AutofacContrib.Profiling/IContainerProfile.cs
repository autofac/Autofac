using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;

namespace AutofacContrib.Profiling
{
    public interface IContainerProfile
    {
        IEnumerable<KeyValuePair<IComponentRegistration, int>> ActivationCounts
        {
            get;
        }
    }
}
