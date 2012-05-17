using System;
using System.Collections.Generic;

namespace Autofac.Extras.Profiling
{
    public interface IContainerProfile
    {
        ComponentRegistrationInfo GetComponent(Guid id);
        IEnumerable<ComponentRegistrationInfo> Components { get; }
     }
}
