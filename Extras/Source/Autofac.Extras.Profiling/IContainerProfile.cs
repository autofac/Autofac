using System;
using System.Collections.Generic;

namespace AutofacContrib.Profiling
{
    public interface IContainerProfile
    {
        ComponentRegistrationInfo GetComponent(Guid id);
        IEnumerable<ComponentRegistrationInfo> Components { get; }
     }
}
